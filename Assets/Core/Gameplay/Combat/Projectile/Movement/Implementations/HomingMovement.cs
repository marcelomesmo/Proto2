using UnityEngine;

namespace Core.Gameplay.Combat.Projectile.Movement.Implementations
{
    [CreateAssetMenu(fileName = "HomingMovement", menuName = "Combat/Projectile/Movement/Homing")]
    public class HomingMovement : ProjectileMovement
    {
        public Transform target;      // Target to home in on
        public float turnSpeed = 5f;  // How quickly it turns toward the target

        public override MovementContext CreateContext()
        {
            return new HomingMovementContext(target, turnSpeed, speed);
        }
    
        // Per-instance context (runtime state)
        private class HomingMovementContext : MovementContext
        {
            private Transform _target;
            private float _turnSpeed;
            private float _projectileSpeed;
        
            public HomingMovementContext(Transform target, float turnSpeed, float projectileSpeed)
            {
                _target = target;
                _turnSpeed = turnSpeed;
                _projectileSpeed = projectileSpeed;
            }
        
            public override void Initialize(Rigidbody2D rb, Vector2 direction, float range)
            {
                if (!_target)
                    return;
        
                Vector2 targetDirection = ((Vector2)_target.position - rb.position).normalized;
                rb.linearVelocity = targetDirection * _projectileSpeed;
            }

            public override void Move(Rigidbody2D rb, Vector2 direction)
            {
                if (!_target)
                    return;

                Vector2 targetDirection = ((Vector2)_target.position - rb.position).normalized;
                Vector2 currentDir = rb.linearVelocity.normalized;

                // Smoothly rotate velocity toward the target
                Vector2 newDir = Vector2.Lerp(currentDir, targetDirection, _turnSpeed * Time.fixedDeltaTime).normalized;

                rb.linearVelocity = newDir * _projectileSpeed;

                // Optional: rotate bullet to face movement direction
                float angle = Mathf.Atan2(newDir.y, newDir.x) * Mathf.Rad2Deg;
                rb.rotation = angle;
            }
        }
    }
}
