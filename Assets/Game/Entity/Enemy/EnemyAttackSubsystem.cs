using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.Projectile;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Subsystem;
using Core.Services.Manager;
using Enemy;
using UnityEngine;

namespace Entity
{
    // TODO: Player does this through the WeaponManager, need refactor?
    public class EnemyAttackSubsystem : EntityAttackSubsystem
    {
        [Header("Melee Attack")]
        public LayerMask enemyLayers;
        private Vector2 boxSize = new(3.0f, 0.5f);
        public AttackData attacks;  // TODO: Make this a list and use the corresponding attack when desired.

        private int _facingDirection = 1;
        private Vector2 _attackDirection;
        private float _attackAngle;

        private EnemyStats _stats;
        private EnemyController _controller;
        
        private DamageSource _damageSource;

        protected override void OnInitialize()
        {
            if (Controller.Stats is EnemyStats enemyStats)
                _stats = enemyStats;
            else
                Debug.LogError("[EntityAttack] requires EnemyStats!");
            
            if (Controller is EnemyController enemyController)
                _controller = enemyController;
            else
                Debug.LogError("[EntityAttack] requires EnemyController!");

            boxSize = new Vector2(_stats.attackRange, 0.5f);    // TODO: Create a box height variable in stats later if necessary, for different hit boxes shapes/sizes. E.g. big beam that cover screen, etc. Might only need if we add jump though.
        }
        
        public void Attack(EntityController target)
        {
            // Example: trigger animation, spawn projectile, or apply damage
            // Debug.Log($"{name} attacks!");
            
            // Play attack animation
            if (Controller.Animator)
                Controller.Animator.SetTrigger("attack");
            
            if (_stats.isRanged)
                ShootProjectile(target);
            else
                OnAttackStarted(target);//MeleeHit(Controller.Target);
        }

        private void ShootProjectile(EntityController target)
        {
            // Shoot logic
            
            // Find the attack direction for the projectile.
            _attackDirection = (target.transform.position - transform.position).normalized;
            
            if (_stats.movementType == EnemyMovementType.Flyer)
            {
                // Flyers shoot directly towards the target.
                _attackAngle = Mathf.Atan2(_attackDirection.y, _attackDirection.x) * Mathf.Rad2Deg;
            }
            else
            {
                // Ground enemies: left = 180°, right = 0°.
                _attackAngle = _attackDirection.x < 0 ? 180f : 0f;
            }
            
            // Spawn the projectile and propel in the correct direction.
            BaseProjectile bulletObject = ProjectilePoolManager.Instance.Spawn(_stats.projectilePrefab);
            bulletObject.transform.position = transform.position;
            
           // ProjectileContext bulletContext = new ProjectileContext(Faction.Enemy, _stats.attackRange, 0, 0);  
            
            var context = new ProjectileContext(
                faction: Faction.Enemy,
                range: _stats.attackRange,
                bonusPierce: 0,//upgrades.pierce,           // TODO: Replace this with Entity upgrades later on.
                damageMultiplier: 0//stats.damageMultiplier
            );

            _damageSource = new DamageSource(
                faction: Faction.Enemy,
                controller: _controller,
                sourcePosition: transform.position
            );
            
            bulletObject.Configure(context, _damageSource);
            
            bulletObject.Launch(_attackAngle);
        }

        // Align to the right direction
        private void OnAttackStarted(EntityController target)
        {
            // Get direction to target
            Vector2 direction = (target.transform.position - transform.position).normalized;
            _facingDirection = 1;
            if(direction.x < 0) _facingDirection = -1;
        }

        // Called via animation event
        public override void OnAttackHit()
        {
            // TODO: Change this to a public variable instead?
            float overlapTolerance = 0.1f;
            
            Bounds bounds = GetEntityBounds();
            float enemyHalfWidth = bounds.extents.x;
            float boxHalfWidth = _stats.attackRange * 0.5f; //boxSize.x * 0.5f;

            // Enemy’s center in world coords
            Vector2 origin = bounds.center;
            
            // Best hitbox positioning: flush but slightly inside
            float offset = (enemyHalfWidth - overlapTolerance) + boxHalfWidth;

            // Compute hitbox center directly in front of the enemy
            Vector2 boxCenter = origin + new Vector2(offset * _facingDirection, 0f);
            
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                boxCenter,
                boxSize,
                0f,
                enemyLayers
            );

            foreach (var enemy in hits)
            {
                if (enemy.GetComponent<EntityController>()?.IsDead == false)
                {
                    // TODO: When having multiple AttackData (List), need to refactor this to pass the appropriate one, i.e. the actual attack used (from the list).
                    var damageSourcePayload = new DamagePayload(
                        attacks,
                        Vector2.zero,
                        _damageSource);
                    
                    enemy.GetComponent<EntityHealth>()?.TakeDamage(damageSourcePayload);
                }
            }
        }
        
        private Bounds GetEntityBounds()
        {
            // TODO: Cache this if needed.
            // Priority: Collider2D > SpriteRenderer
            if (TryGetComponent<Collider2D>(out var col))
                return col.bounds;

            if (TryGetComponent<SpriteRenderer>(out var sr))
                return sr.bounds;

            // fallback (rare)
            return new Bounds(transform.position, Vector3.one);
        }
        
        void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
                return;
            
            Bounds bounds = GetEntityBounds();
            float enemyHalfWidth = bounds.extents.x;
            float boxHalfWidth   = boxSize.x * 0.5f;
            Vector2 origin = bounds.center;

            // Preview hitbox
            Vector2 boxCenter = origin + new Vector2((enemyHalfWidth + boxHalfWidth) * _facingDirection, 0f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boxCenter, boxSize);
        }
    }
}