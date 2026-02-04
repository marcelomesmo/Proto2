using System;
using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.AreaEffect;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.Modifiers;
using Core.Gameplay.Combat.Projectile;
using Core.Gameplay.Entity.Attack;
using Core.Gameplay.Entity.Stats;
using Core.Interfaces;
using Core.Services;
using Core.Services.Manager;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Gameplay.Entity.Subsystem
{
    [RequireComponent(typeof(EntityAttackLoadout))]
    public class EntityAttackSubsystem : BaseSubsystem
    {
        [Header("Attack Settings")]
        [SerializeField] private LayerMask hitLayers;
        
        private readonly Dictionary<AttackData, AttackInstance> _attacks = new();
        public IReadOnlyDictionary<AttackData, AttackInstance> Attacks => _attacks;
        
        // Helpers
        private BaseEntityStats _stats;
        private EntityController _controller;
        private EntityAttackLoadout _attackLoadout;
        private AttackInstance _currentAttack;
        private AttackContext _pendingContext;
        private EntityPresentationSubsystem _presentation;
        
        private Vector2 _boxSize = new(3f, 0.5f);
        private AttackTargetFilter _targetFilter;

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
            
            _attacks.Clear();
            
            _attackLoadout.OnLoadoutChanged += RebuildAttackInstances;
            RebuildAttackInstances();

            BuildTargetFilter();
        }

        protected override void OnDeinitialize()
        {
            // Q: is it better to do this here or let HandleAttackResolveTimeout resolve?
            _currentAttack = null;
            _pendingContext = default;
            _waitingForResolve = false;
        }

        protected override void OnUpdate()
        {
            OnTick(Time.deltaTime);
            
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
        
        public bool TryExecute(AttackData attack, AttackContext context)
        {
            if (attack == null)
                return false;
            
            if (!_attacks.TryGetValue(attack, out var instance))
                return false;

            if (!instance.IsReady)
                return false;

            if (!IsTargetInRange(attack, context))
                return false;

            ExecuteAttack(instance, context);
            instance.Consume();

            return true;
        }
        
        private void ExecuteAttack(AttackInstance attack, AttackContext context)
        {
            _currentAttack = attack;
            _pendingContext = context;
            
            _waitingForResolve = true;              // TEMP (see below HandleAttackResolveTimeout)
            _resolveTimer = attackResolveTimeout;   // TEMP (see below HandleAttackResolveTimeout)
            
            if (Controller.Animator)
                Controller.Animator.SetTrigger("attack");

            OnAttackExecuted?.Invoke(attack);
        }

        // Called via animation event
        public void AttackResolveFrame()
        {
            if (_currentAttack == null)
                return;

            switch (_currentAttack.Data.executionMode)
            {
                case AttackExecutionMode.Melee:
                    ApplyMeleeHit(_currentAttack.Data);
                    break;

                case AttackExecutionMode.Projectile:
                    ShootProjectile(_currentAttack.Data, _pendingContext);
                    break;

                case AttackExecutionMode.AreaEffect:
                    SpawnAreaEffect(_currentAttack.Data, _pendingContext);
                    break;
            }

            _currentAttack = null;
            _pendingContext = default; // or null if it's a class
            
            _waitingForResolve = false;             // TEMP (see below HandleAttackResolveTimeout)
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
            
#if UNITY_EDITOR
            Debug.LogError(
                $"[Attack] Attack '{_currentAttack.Data.name}' on '{name}' never resolved.\n" +
                $"Did you forget the OnAttackHit animation event?",
                this);
#endif

            // Fail safe: cancel the attack so the entity doesn't soft-lock
            _currentAttack = null;
            _pendingContext = default;
            _waitingForResolve = false;
        }

        #region AttackExecutionMode : Projectile
        
        private void ShootProjectile(AttackData data, AttackContext context)
        {
            if (context.Direction.sqrMagnitude < 0.0001f)
            {
                Debug.LogWarning(
                    $"[EntityAttackSubsystem] Invalid projectile direction on {Controller.name}",
                    this);
            }

            // Find the correct angle.
            float angle;
            switch (data.directionMode)
            {
                case ProjectileDirectionMode.UseAttackDirection:
                    angle = Mathf.Atan2(context.Direction.y, context.Direction.x) * Mathf.Rad2Deg;
                    break;

                case ProjectileDirectionMode.FixedAngle:
                    angle = data.fixedAngle;
                    break;

                case ProjectileDirectionMode.HorizontalFacing:
                default:
                    angle = context.Direction.x < 0 ? 180f : 0f;
                    break;
            }

            var projectile = ProjectilePoolManager.Instance.Spawn(data.projectilePrefab);
            
            // Spawn at CastAnchor when available.
            var spawnTransform =
                _presentation && _presentation.CastAnchor
                    ? _presentation.CastAnchor
                    : transform;

            projectile.transform.SetPositionAndRotation(
                spawnTransform.position,
                spawnTransform.rotation
            );
            
            var damageSource = new DamageSource(
                _stats.faction,
                _controller,
                transform.position,
                _targetFilter
            );
            
            projectile.Configure(CreateProjectileContext(data), damageSource);
            projectile.Launch(angle);
        }
        
        #endregion
        
        #region AttackExecutionMode : AreaEffect
        
        private void SpawnAreaEffect(AttackData data, AttackContext context)
        {
            if (!data.areaEffectData)
            {
                Debug.LogWarning($"Attack {data.name} has no AreaEffectData");
                return;
            }
            
            // 1. Define Spawn position
            var spawnTransform = ResolveAreaSpawnTransform(data.areaEffectData);
            var spawnPosition = ResolveAreaSpawnPosition(data.areaEffectData, context);

            // todo: add pooling here in the future: AreaEffectPoolManager.Instance.Spawn(data.areaEffectPrefab)
            var go = new GameObject($"AreaEffect_{data.name}");
            
            // 2. Define Follow mode and apply position
            if (data.areaEffectData.followOwner)
            {
                go.transform.SetParent(spawnTransform, worldPositionStays: false);
                go.transform.localPosition =
                    spawnTransform.InverseTransformPoint(spawnPosition);
            }
            else
            {
                go.transform.position = spawnPosition;
            }

            var instance = go.AddComponent<AreaEffectInstance>();

            var modifiers = ListPool<DamageModifier>.Get();
            
            ServiceLocator
                .Get<GameController>()?
                .UpgradeManager
                .CollectDamageModifiers(
                    ModifierScope.Area,
                    modifiers);
            
            var damageSource = new DamageSource(
                _stats.faction,
                _controller,
                transform.position,     // go.transform.position?
                _targetFilter
            );
            
            var attackPower = GetAttackPowerBonus();
            
            var payload = new DamagePayload(
                hitData: data,
                baseDamage: data.damage + attackPower,
                modifiers: modifiers,
                effects: data.Effects,
                hitPoint: go.transform.position,
                source: damageSource
            );

            instance.Initialize(data.areaEffectData, payload);
            
            ListPool<DamageModifier>.Release(modifiers);
        }
        
        private Transform ResolveAreaSpawnTransform(AreaEffectData data)
        {
            if (_presentation && _presentation.CastAnchor)
                return _presentation.CastAnchor;

            return transform;
        }
        
        private Vector3 ResolveAreaSpawnPosition(
            AreaEffectData data,
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
        
        private void ApplyMeleeHit(AttackData data)
        {
            Bounds bounds = GetEntityBounds();

            float halfWidth = bounds.extents.x;
            float boxHalf = data.range * 0.5f;

            int facingDirection = Controller.GetComponent<EntityPresentationSubsystem>().CurrentFacing == FacingDirection.Right ? 1 : -1;
            
            Vector2 origin = bounds.center;
            Vector2 center = origin + new Vector2((halfWidth + boxHalf) * facingDirection, 0f);

            _boxSize.x = data.range;

            _meleeHits.Clear();

            var filter = _targetFilter.ToContactFilter();

            Physics2D.OverlapBox(
                center,
                _boxSize,
                0f,
                filter,
                _meleeHits
            );

            foreach (var hit in _meleeHits)
            {
                if (!hit)
                    continue;
                
                if (!_targetFilter.CanHit(hit))
                    continue;
                
                if (!hit.TryGetComponent<IDamageable>(out var damageable))
                    continue;
                
                if (!damageable.CanBeDamaged())
                    continue;
                
                var modifiers = ListPool<DamageModifier>.Get();

                ServiceLocator
                    .Get<GameController>()?
                    .UpgradeManager
                    .CollectDamageModifiers(
                        ModifierScope.Melee,
                        modifiers);
                
                var damageSource = new DamageSource(
                    _stats.faction,
                    _controller,
                    transform.position,
                    _targetFilter
                );
                
                var attackPower = GetAttackPowerBonus();
                
                var payload = new DamagePayload(
                    hitData: data,
                    baseDamage: data.damage + attackPower,
                    modifiers: null,
                    effects: data.Effects,
                    hitPoint: Vector2.zero,
                    source: damageSource
                );
                
                ListPool<DamageModifier>.Release(modifiers);
                
                damageable.TakeDamage(payload);
            }
        }
        
        #endregion
        
        #region Util
        
        private ProjectileContext CreateProjectileContext(AttackData data)
        {
            return new ProjectileContext(
                faction: _stats.faction,
                range: data.range,
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
        
        private bool IsTargetInRange(AttackData data, AttackContext context)
        {
            if (context.Target == null)
                return true; // Directional or blind attack
            
            float distance = Vector2.Distance(
                transform.position,
                context.Target.transform.position
            );

            return distance <= data.range;
        }

        public bool CanExecute(AttackData data) => _attacks.ContainsKey(data);

        private void BuildTargetFilter()
        {
            _targetFilter = new AttackTargetFilter
            {
                layerMask = hitLayers,
                allowTriggers = true
            };
        }
        
        public void RebuildTargetFilter(LayerMask newMask, bool allowTriggers)
        {
            _targetFilter.layerMask = newMask;
            _targetFilter.allowTriggers = allowTriggers;
        }
        
        private int GetAttackPowerBonus()
        {
            if (Controller.Stats is Game.Entity.Player.Stats.CharacterStats stats)
                return Mathf.RoundToInt(stats.attackPower);
            
            // Simple for now, but later we can do:
            //  stats.attackPower + _temporaryAttackBuff, or
            //  stats.attackPower * (IsEnraged ? 2 : 1); or
            //  stats.attackPower * stats.attackPowerMultiplier; etc.

            return 0;
        }
        
        #endregion
        
        #region Attack Loadout

        private void RebuildAttackInstances()
        {
            _attacks.Clear();
            
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

                _attacks.Add(attack, new AttackInstance(attack));
            }
        }
        
        #endregion
    }
}
