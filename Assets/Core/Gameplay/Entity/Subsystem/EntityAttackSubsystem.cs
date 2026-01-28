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
        
        // Helpers
        private BaseEntityStats _stats;
        private EntityController _controller;
        private EntityAttackLoadout _attackLoadout;
        private DamageSource _damageSource;
        private AttackInstance _currentAttack;
        
        private Vector2 _boxSize = new(3f, 0.5f);

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
            
            _attackLoadout = GetComponent<EntityAttackLoadout>();
            
            _attacks.Clear();
            
            _attackLoadout.OnLoadoutChanged += RebuildAttackInstances;
            RebuildAttackInstances();

            CreateDamageSource();
        }

        protected override void OnUpdate()
        {
            OnTick(Time.deltaTime);
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
            
            if (Controller.Animator)
                Controller.Animator.SetTrigger("attack");

            OnAttackExecuted?.Invoke(attack);

            switch (attack.Data.executionMode)
            {
                case AttackExecutionMode.Projectile:
                    ShootProjectile(attack.Data, context);
                    break;

                case AttackExecutionMode.AreaEffect:
                    SpawnAreaEffect(attack.Data, context);
                    break;

                case AttackExecutionMode.Melee:
                    // Called via animation event
                default:
                    break;
            }
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
            projectile.transform.position = transform.position;
            
            projectile.Configure(CreateProjectileContext(data), _damageSource);
            projectile.Launch(angle);
        }
        
        #endregion
        
        #region Area
        
        private void SpawnAreaEffect(AttackData data, AttackContext context)
        {
            if (!data.areaEffectData)
            {
                Debug.LogWarning($"Attack {data.name} has no AreaEffectData");
                return;
            }

            // todo: add pooling here in the future: AreaEffectPoolManager.Instance.Spawn(data.areaEffectPrefab)
            var go = new GameObject($"AreaEffect_{data.name}");
            go.transform.position = context.Target
                ? context.Target.transform.position
                : transform.position;

            var instance = go.AddComponent<AreaEffectInstance>();
            // todo: also don't have the vfx for the effect instantianted as we dont instantiate a prefab

            var modifiers = ListPool<DamageModifier>.Get();
            
            ServiceLocator
                .Get<GameController>()?
                .UpgradeManager
                .CollectDamageModifiers(
                    ModifierScope.Area,
                    modifiers);
            
            var payload = new DamagePayload(
                hitData: data,
                baseDamage: data.damage,
                modifiers: modifiers,
                effects: data.Effects,
                hitPoint: go.transform.position,
                source: CreateDamageSource()
            );

            instance.Initialize(data.areaEffectData, payload);
            
            ListPool<DamageModifier>.Release(modifiers);
        }
        
        #endregion
        
        #region AttackExecutionMode : Melee
        
        // Called via animation event
        public void OnAttackHit()
        {
            if (_currentAttack == null || _currentAttack.Data.executionMode != AttackExecutionMode.Melee)
                return;

            ApplyMeleeHit(_currentAttack.Data);
            _currentAttack = null;
        }
        
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
            
            var filter = new ContactFilter2D
            {
                useTriggers = true,
                layerMask = hitLayers
            };

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
                
                var payload = new DamagePayload(
                    hitData: data,
                    baseDamage: data.damage,
                    modifiers: null,
                    effects: data.Effects,
                    hitPoint: Vector2.zero,
                    source: CreateDamageSource()
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
        
        private DamageSource CreateDamageSource()
        {
            return new DamageSource(
                _stats.faction,
                _controller,
                transform.position
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
