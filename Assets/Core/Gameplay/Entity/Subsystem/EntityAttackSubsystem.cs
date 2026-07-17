using System;
using System.Collections;
using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat;
using Core.Gameplay.Combat.AreaAttack;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.Projectile;
using Core.Gameplay.Entity.Attack;
using Core.Gameplay.Entity.Stats;
using Core.Gameplay.Entity.Tags;
using Core.Interfaces;
using Core.Services.Manager;
using UnityEngine;

namespace Core.Gameplay.Entity.Subsystem
{
    [RequireComponent(typeof(EntityAttackLoadout))]
    [RequireComponent(typeof(EntityModifierSubsystem))]
    public class EntityAttackSubsystem : BaseSubsystem
    {
        [Header("Attack Settings")]
        [SerializeField] private LayerMask hitLayers;
        
        private readonly Dictionary<AttackData, AttackInstance> _attacks = new();
        public IReadOnlyDictionary<AttackData, AttackInstance> Attacks => _attacks;
        private readonly Dictionary<AttackData, AttackInstance> _variantInstances = new();
        
        private readonly Dictionary<AttackData, List<StatusEffectData>> _runtimeAttackEffects = new();
        
        // Active Attack State
        public bool IsAttackChanneling => _channeledAttack != null;
        public bool IsAttackInProgress => _currentAttack != null || _channeledAttack != null;
        private AttackInstance _channeledAttack;
        private float _channeledAttackTimer;
        private Coroutine _extraExecutionRoutine;
        private int _remainingExtraExecutions;
        
        // Helpers - cached
        private BaseEntityStats _stats;
        private EntityController _controller;
        private EntityAttackLoadout _attackLoadout;
        private EntityModifierSubsystem _modifiers;
        private AttackInstance _sourceAttack;
        private AttackInstance _currentAttack;
        private AttackContext _pendingContext;
        private EntityPresentationSubsystem _presentation;
        
        private Vector2 _boxSize = new(3f, 0.5f);
        //private AttackTargetFilter _targetFilter;

        public event Action<AttackInstance> OnAttackExecuted;
        
        protected override void OnInitialize()
        {
            if (Controller.Stats)
                _stats = Controller.Stats;
            else
                Debug.LogError("[EntityAttackSubsystem] requires Stats!");
            
            if (Controller)
                _controller = Controller;
            else
                Debug.LogError("[EntityAttackSubsystem] requires Controller!");
            
            _presentation = Controller.GetComponent<EntityPresentationSubsystem>();
            
            _attackLoadout = GetComponent<EntityAttackLoadout>();
            
            _modifiers = GetComponent<EntityModifierSubsystem>();
            
            _attacks.Clear();
            
            _attackLoadout.OnLoadoutChanged += RebuildAttackInstances;
            RebuildAttackInstances();

            //BuildTargetFilter();
        }

        protected override void OnDeinitialize()
        {
            // avoid lingering coroutine
            if (_extraExecutionRoutine != null)
            {
                StopCoroutine(_extraExecutionRoutine);
                _extraExecutionRoutine = null;
            }
            // Q: is it better to do this here or let HandleAttackResolveTimeout resolve?
            _sourceAttack = null;
            _currentAttack = null;
            _channeledAttack = null;
            _pendingContext = default;
            _waitingForResolve = false;

            _runtimeAttackEffects.Clear();
        }
        
        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == Controller.Stats.deadTag ||
                tag == Controller.Stats.matchEndedTag ||
                tag == Controller.Stats.stunTag ||
                tag == Controller.Stats.knockbackTag)
            {
                CancelAnyAttackSequence();
            }
        }

        protected override void OnUpdate()
        {
            OnTick(Time.deltaTime);
            
            UpdateActiveAttack(Time.deltaTime);
            
            if (_waitingForResolve)             // TEMP (see below HandleAttackResolveTimeout)
            {
                _resolveTimer -= Time.deltaTime;

                if (_resolveTimer <= 0f)
                    HandleAttackResolveTimeout();
            }
        }
        
        private void OnTick(float deltaTime)
        {
            // Tick all attack instances (that are currently running).
            foreach (var attack in _attacks.Values)
                attack.Tick(deltaTime);
        }
        
        private void UpdateActiveAttack(float deltaTime)
        {
            if (_channeledAttack == null)
                return;

            _channeledAttackTimer -= deltaTime;

            if (_channeledAttackTimer <= 0f)
                EndChanneledAttack();
        }
        
        public bool TryExecute(AttackData attack, AttackContext context)
        {
            if (attack == null)
                return false;
            
            // IsAttackInProgress Safeguard to prevent new attacks while performing one attack. Shouldn't be needed if check is properly done in Brain.
            // Is attack is passive we bypass this check.
            if (IsAttackInProgress && !attack.isPassive)
                return false;
            
            if (!_attacks.TryGetValue(attack, out var instance))
                return false;

            if (!instance.IsReady)
                return false;

            if (!IsTargetInRange(instance, context))
                return false;

            AttackInstance resolvedAttack  =
                instance.GetResolvedAttackVariant(
                    context,
                    this);
            
            // Passive attacks need to be Direct
            if (attack.isPassive &&
                resolvedAttack.Data.executionMode == AttackExecutionMode.Direct)
            {
                ExecuteDirectCast(
                    resolvedAttack,
                    context);

                instance.Consume();

                OnAttackExecuted?.Invoke(resolvedAttack);

                return true;
            }
            
            ExecuteAttack(
                sourceAttack: instance,
                executionAttack: resolvedAttack,
                context: context);
            
            instance.Consume();

            return true;
        }
        
        private void ExecuteAttack(
            AttackInstance sourceAttack,
            AttackInstance executionAttack,
            AttackContext context)
        {
            _sourceAttack = sourceAttack;
            _currentAttack = executionAttack;
            _pendingContext = context;
            _remainingExtraExecutions = executionAttack.GetResolvedExtraExecutions();
            
            _waitingForResolve = true;              // TEMP (see below HandleAttackResolveTimeout)
            _resolveTimer = attackResolveTimeout;   // TEMP (see below HandleAttackResolveTimeout)
            
            // TODO: Later we should maybe do Controller.Tags.AddTag(attackingTag) and let the PresentationSubsystem handle this?
            if (Controller.Animator && !_currentAttack.Data.castSilently)
                Controller.Animator.SetTrigger("attack");

            OnAttackExecuted?.Invoke(executionAttack);
        }

        // Called via animation event
        public void AttackResolveFrame()
        {
            if (_currentAttack == null)
                return;
            
            bool keepAttackAlive = false;

            switch (_currentAttack.Data.executionMode)
            {
                case AttackExecutionMode.Melee:
                    ApplyMeleeHit(_currentAttack);
                    break;

                case AttackExecutionMode.Projectile:
                    ShootProjectile(_currentAttack, _pendingContext);
                    break;

                case AttackExecutionMode.Area:
                    keepAttackAlive =
                        SpawnAreaEffect(_currentAttack, _pendingContext);
                    break;
                
                case AttackExecutionMode.Direct:
                    ExecuteDirectCast(_currentAttack, _pendingContext);
                    break;
            }
            
            _waitingForResolve = false;             // TEMP (see below HandleAttackResolveTimeout)
            
            // Q: Maybe we could refactor so every attack is added to BeginActiveAttack with duration = 0 for instant attacks?
            if (!keepAttackAlive)
            {
                if (_remainingExtraExecutions > 0)
                {
                    _remainingExtraExecutions--;

                    _extraExecutionRoutine = 
                        StartCoroutine(ExecuteNextAttack());

                    return;
                }
                    
                EndAttackSequence();
            }
        }
        
        //
        //  Scheduled Executions Handling
        //
        private const float DelayBetweenExtraExecutions = 0.15f;
        private IEnumerator ExecuteNextAttack()
        {
            yield return new WaitForSeconds(DelayBetweenExtraExecutions);
            
            _extraExecutionRoutine = null;
            
            if (_currentAttack == null)
                yield break;

            if (Controller == null || Controller.IsDead)
            {
                EndAttackSequence();
                yield break;
            }
            
            _waitingForResolve = true;
            _resolveTimer = attackResolveTimeout;
            
            if (Controller.Animator)
                Controller.Animator.SetTrigger("attack");
        }
        private void EndAttackSequence()
        {
            _sourceAttack = null;
            _currentAttack = null;
            _pendingContext = default;

            _remainingExtraExecutions = 0;

            _waitingForResolve = false;
            _resolveTimer = 0f;
            
            _extraExecutionRoutine = null;
        }
        
        private void CancelAnyAttackSequence()
        {
            if (_extraExecutionRoutine != null)
            {
                StopCoroutine(_extraExecutionRoutine);
                _extraExecutionRoutine = null;
            }

            EndAttackSequence();

            InterruptChannel();
        }

        //
        // ATTACK RESOLVE TIMEOUT HANDLING
        //
        // Temporarily used for testing. Later should be a serializedfield tuned per project.
        private float attackResolveTimeout = 1.0f; // seconds (tune per project)

        private float _resolveTimer;
        private bool _waitingForResolve;
        
        private void HandleAttackResolveTimeout()
        {
            if (Controller.IsDead)
                return;
            
            if (_currentAttack == null)
            {
                _waitingForResolve = false;
                return;
            }
            
#if UNITY_EDITOR
            Debug.LogError(
                $"[Attack] Attack '{_currentAttack.Data.name}' from '{name}' cast on '{_pendingContext.Target}' never resolved.\n" +
                $"Did you forget the OnAttackHit animation event?",
                this);
#endif
            
            EndAttackSequence();
        }

        #region AttackExecutionMode : Projectile
        
        private void ShootProjectile(AttackInstance attack, AttackContext context)
        {
            // 1. Resolve the actual projectile origin first.
            // Spawn at CastAnchor when available.
            var spawnTransform =
                _presentation && _presentation.CastAnchor
                    ? _presentation.CastAnchor
                    : transform;
            Vector2 spawnPosition = spawnTransform.position;

            // 2. Resolve the final projectile direction.
            Vector2 launchDirection;

            switch (attack.Data.directionMode)
            {
                case ProjectileDirectionMode.UseAttackDirection:
                {
                    // A targeted projectile must calculate its direction from
                    // its actual spawn point, not from the entity root.
                    if (context.Target)
                    {
                        Vector2 targetPosition =
                            context.Target.transform.position;

                        launchDirection =
                            targetPosition - spawnPosition;
                    }
                    else
                    {
                        // Preserve the Brain-provided direction for blind or
                        // otherwise targetless attacks.
                        launchDirection = context.Direction;
                    }

                    break;
                }

                case ProjectileDirectionMode.FixedAngle:
                {
                    float radians =
                        attack.Data.fixedAngle * Mathf.Deg2Rad;

                    launchDirection = new Vector2(
                        Mathf.Cos(radians),
                        Mathf.Sin(radians)
                    );

                    break;
                }

                case ProjectileDirectionMode.HorizontalFacing:
                default:
                {
                    // CastAnchor is already rotated by the presentation subsystem.
                    launchDirection = spawnTransform.right;
                    break;
                }
            }
            
            if (context.Direction.sqrMagnitude < 0.0001f)
            {
                Debug.LogWarning(
                    $"[EntityAttackSubsystem] Invalid projectile direction on {Controller.name}",
                    this);
            }
            
            launchDirection.Normalize();

            float angle =
                Mathf.Atan2(
                    launchDirection.y,
                    launchDirection.x
                ) * Mathf.Rad2Deg;

            // 3. Spawn pooled projectile
            var projectile = ProjectilePoolManager.Instance.Spawn(attack.Data.projectilePrefab);

            projectile.transform.SetPositionAndRotation(
                spawnPosition,              // Adjusted to match shift caused by CastAnchor
                spawnTransform.rotation
            );
            
            // 4. Construct the combat data.
            var targetFilter = BuildAttackTargetFilter(attack.Data);
            
            var combatSource = new AttackSource(
                _stats.faction,
                _controller,
                transform.position,
                targetFilter
            );
            
            // Projectiles have no single target at cast time; crit is rolled per-hit inside the projectile.
            // We resolve against null target here so the projectile carries a pre-rolled crit from the caster.
            var result = GetResolvedCombatAmount(attack);
            
            var payload = CombatPayloadFactory.Create(
                attack: attack,
                amount: result.amount,
                flags: result.flags,
                scope: ModifierScope.Projectile,
                source: combatSource
            );
            
            projectile.Configure(CreateProjectileContext(attack), combatSource, payload, _pendingContext.Target);
            projectile.Launch(angle);
        }
        
        #endregion
        
        #region AttackExecutionMode : AreaEffect
        
        private bool SpawnAreaEffect(AttackInstance attack, AttackContext context)
        {
            if (!attack.Data.areaAttackData)
            {
                Debug.LogWarning($"Attack {attack.Data.name} is set to ExecuteMode:AreaEffect but has no AreaEffectData");
                return false;
            }
            
            // 1. Resolve spawn transform/position
            var spawnTransform = ResolveAreaSpawnTransform(attack.Data.areaAttackData);
            var spawnPosition = ResolveAreaSpawnPosition(attack.Data.areaAttackData, context);

            // 2. Spawn pooled effect
            var areaEffect = AreaEffectPoolManager.Instance.Spawn(attack.Data.areaAttackData.areaEffectPrefab);
            
            if (!areaEffect)
                return false;
            
            // Important:
            // pooled transforms may still contain previous state
            areaEffect.transform.SetParent(null);

            // Reset transform state
            areaEffect.transform.SetPositionAndRotation(
                spawnPosition,
                Quaternion.identity);
            areaEffect.transform.localScale = Vector3.one;
            
            // 3. Follow owner handling
            if (attack.Data.areaAttackData.followOwner)
            {
                areaEffect.transform.SetParent(
                    spawnTransform,
                    worldPositionStays: true);
            }
            
            // 4. Damage source
            var targetFilter = BuildAttackTargetFilter(attack.Data);
            
            var damageSource = new AttackSource(
                _stats.faction,
                _controller,
                areaEffect.transform.position,
                targetFilter
            );
            
            // Area effects hit multiple targets over time; crit is rolled once at spawn and shared accross all hits.
            var result = GetResolvedCombatAmount(attack);
            
            var payload = CombatPayloadFactory.Create(
                attack: attack,
                amount: result.amount,
                flags: result.flags,
                scope: ModifierScope.Area,
                source: damageSource
            );

            // 5. Initialize
            areaEffect.Initialize(
                owner: _controller,
                data: attack.Data.areaAttackData,
                payload: payload
            );

            // 6 Begin channeling
            if (attack.Data.areaAttackData.isChanneled)
            {
                BeginChanneledAttack(_currentAttack);
                return true;
            }

            return false;
        }
        
        private void BeginChanneledAttack(AttackInstance attack)
        {
            _channeledAttack = attack;
            _channeledAttackTimer = attack.GetDuration();

            //
            //  Lock movement/brain-facing
            //  (this is done in the Brain)
            //
           
            if (Controller.Animator)
                Controller.Animator.SetBool("isChanneling", true);

            if (_presentation)
                _presentation.LockFacing(true);
        }
        
        private void EndChanneledAttack()
        {
            if (Controller.Animator)
                Controller.Animator.SetBool("isChanneling", false);
            
            if (_presentation)
                _presentation.LockFacing(false);

            _channeledAttack = null;
            
            _sourceAttack = null;
            _currentAttack = null;

            _pendingContext = default;
        }
        
        public void InterruptChannel()
        {
            if (_channeledAttack == null)
                return;

            EndChanneledAttack();
        }
        
        private Transform ResolveAreaSpawnTransform(AreaAttackData data)
        {
            if (_presentation && _presentation.CastAnchor)
                return _presentation.CastAnchor;

            return transform;
        }
        
        private Vector3 ResolveAreaSpawnPosition(
            AreaAttackData data,
            AttackContext context)
        {
            var origin = ResolveAreaSpawnTransform(data);

            switch (data.spawnMode)
            {
                case AreaSpawnMode.WorldPosition:
                    return context.Target
                        ? context.Target.transform.position
                        : origin.position;

                case AreaSpawnMode.AtCaster:
                    return origin.position;

                case AreaSpawnMode.InFrontOfCaster:
                {
                    var facing =
                        _presentation.CurrentFacing == FacingDirection.Right
                            ? Vector2.right
                            : Vector2.left;

                    return origin.position + (Vector3)(facing * data.forwardOffset);
                }

                default:
                    return origin.position;
            }
        }
        
        #endregion
        
        #region AttackExecutionMode : Melee
        
        private readonly List<Collider2D> _meleeHits = new();
        
        private void ApplyMeleeHit(AttackInstance attack)
        {
            Bounds bounds = GetEntityBounds();

            float range = attack.GetRange();
            
            float halfWidth = bounds.extents.x;
            float boxHalf = range * 0.5f;

            int facingDirection = Controller.GetComponent<EntityPresentationSubsystem>().CurrentFacing == FacingDirection.Right ? 1 : -1;
            
            Vector2 origin = bounds.center;
            Vector2 center = origin + new Vector2((halfWidth + boxHalf) * facingDirection, 0f);

            _boxSize.x = range;

            _meleeHits.Clear();

            var targetFilter = BuildAttackTargetFilter(attack.Data);
            var filter = targetFilter.ToContactFilter();

            Physics2D.OverlapBox(
                center,
                _boxSize,
                0f,
                filter,
                _meleeHits
            );
            
            var source = new AttackSource(
                _stats.faction,
                _controller,
                transform.position,
                targetFilter
            );
            
            foreach (var hit in _meleeHits)
            {
                if (!hit)
                    continue;
                
                if (!targetFilter.CanHit(hit))
                    continue;
                
                if (!hit.TryGetComponent<ICombatReceiver>(out var receiver))
                    continue;

                // Resolve per-target so each target gets an independent crit roll.
                var result = GetResolvedCombatAmount(attack);
            
                var payload = CombatPayloadFactory.Create(
                    attack: attack,
                    amount: result.amount,
                    flags: result.flags,
                    scope: ModifierScope.Melee,
                    source: source
                );
                
                CombatExecutionPipeline.Execute(
                    receiver,
                    payload
                );
            }
        }
        
        #endregion
        
        #region AttackExeuctionMode: Direct

        private void ExecuteDirectCast(AttackInstance attack, AttackContext context)
        {
            if (!context.Target)
                return;

            var source = new AttackSource(
                _stats.faction,
                _controller,
                transform.position,
                BuildAttackTargetFilter(attack.Data)
            );
            
            var result = GetResolvedCombatAmount(attack);
            
            var payload = CombatPayloadFactory.Create(
                attack: attack,
                amount: result.amount,
                flags: result.flags,
                scope: ModifierScope.Direct,
                source: source
            );
            
            if(context.Target.TryGetComponent<ICombatReceiver>(out var receiver))
            {
                CombatExecutionPipeline.Execute(
                    receiver,
                    payload
                );
            }
        }
        
        #endregion
        
        #region Util
        
        private ProjectileContext CreateProjectileContext(AttackInstance attack)
        {
            return new ProjectileContext(
                faction: _stats.faction,
                range: attack.GetRange(),
                bonusPierce: 0,
                damageMultiplier: 1f
            );
        }
        
        private Bounds GetEntityBounds()
        {
            if (TryGetComponent(out Collider2D col))
                return col.bounds;

            if (TryGetComponent(out SpriteRenderer sr))
                return sr.bounds;

            return new Bounds(transform.position, Vector3.one);
        }
        
        private bool IsTargetInRange(AttackInstance attack, AttackContext context)
        {
            if (context.Target == null)
                return true; // Directional or blind attack
            
            float distance = Vector2.Distance(
                transform.position,
                context.Target.transform.position
            );

            return distance <= attack.GetRange();
        }

        public bool CanExecute(AttackData data)
        {
            if (IsAttackInProgress)
                return false;

            if (!_attacks.TryGetValue(data, out var instance))
                return false;

            return instance.IsReady;
        }

        private AttackTargetFilter BuildAttackTargetFilter(AttackData attack)
        {
            return new AttackTargetFilter
            {
                layerMask = attack.targetLayers,
                allowTriggers = true,
                targetType = attack.targetType
            };
        }
        
        private CombatAmountResult<int> GetResolvedCombatAmount(AttackInstance attackInstance)
        {
            AttackResolveContext context = 
                new AttackResolveContext 
                {
                    Source = this.Controller,
                    Modifiers = _modifiers
                };
            
            return attackInstance.GetResolvedCombatAmount(context);
        }
        
        // -------------------------------------
        // Runtime Attack Effects (Upgrades)
        // -------------------------------------

        public void RegisterRuntimeEffect(
            AttackData attack,
            StatusEffectData effect)
        {
            if (attack == null || effect == null)
                return;

            if (!_runtimeAttackEffects.TryGetValue(attack, out var list))
            {
                list = new List<StatusEffectData>();
                _runtimeAttackEffects.Add(attack, list);
            }

            if (!list.Contains(effect))
                list.Add(effect);
        }

        public void UnregisterRuntimeEffect(
            AttackData attack,
            StatusEffectData effect)
        {
            if (attack == null || effect == null)
                return;

            if (!_runtimeAttackEffects.TryGetValue(attack, out var list))
                return;

            list.Remove(effect);

            if (list.Count == 0)
                _runtimeAttackEffects.Remove(attack);
        }

        public IReadOnlyList<StatusEffectData> GetCombinedEffects(AttackData attack)
        {
            if (!_runtimeAttackEffects.TryGetValue(attack, out var extra))
                return attack.Effects;

            var combined = new List<StatusEffectData>(
                attack.Effects.Count + extra.Count);

            combined.AddRange(attack.Effects);
            combined.AddRange(extra);

            return combined;
        }
        
        #endregion
        
        #region Attack Loadout

        private void RebuildAttackInstances()
        {
            _attacks.Clear();
            _variantInstances.Clear();
            
            // Initialize attack reference dictionary.
            foreach (var attack in _attackLoadout.Attacks)
            {
                if (_attacks.ContainsKey(attack))
                {
                    Debug.LogWarning(
                        $"[EntityAttackSubsystem] Duplicate Attack reference {attack.name} on {_controller.name}",
                        this);
                    continue;
                }

                _attacks.Add(attack, new AttackInstance(attack, _modifiers));
            }
        }
        
        public AttackInstance GetOrCreateVariantInstance(
            AttackData attackData)
        {
            if (attackData == null)
                return null;

            if (_attacks.TryGetValue(attackData, out var loadoutInstance))
                return loadoutInstance;

            if (_variantInstances.TryGetValue(attackData, out var variantInstance))
                return variantInstance;

            variantInstance = new AttackInstance(
                attackData,
                _modifiers);

            _variantInstances.Add(
                attackData,
                variantInstance);

            return variantInstance;
        }
        
        #endregion
    }
}
