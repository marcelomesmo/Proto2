using UnityEngine;

namespace Core.Gameplay.Combat.Projectile.Movement.Implementations
{
    [CreateAssetMenu(fileName = "SpreadMovement", menuName = "Projectile/Movement/Spread")]
    public class SpreadMovement : ProjectileMovement
    {
        public float sideOffset = 0.5f;
    
        private void OnEnable()
        {
            physicsMode = ProjectilePhysicsMode.Dynamic;
        }
    
        public override MovementContext CreateContext()
        {
            return new SpreadMovementContext(sideOffset, speed);
        }

        private class SpreadMovementContext : MovementContext
        {
            private Vector2 _perpDir;
            private float _appliedOffset;

            private readonly float _sideOffset;
            private readonly float _speed;
        
            public SpreadMovementContext(float sideOffset, float speed)
            {
                _sideOffset = sideOffset;
                _speed = speed;
            }
        
            public override void Initialize(Rigidbody2D rb, Vector2 direction, float range)
            {
                direction.Normalize();
                _perpDir = new Vector2(-direction.y, direction.x);
                _appliedOffset = Random.Range(-1, 1) * _sideOffset;
                rb.linearVelocity = direction * _speed + _perpDir * _appliedOffset;
            }

            public override void Move(Rigidbody2D rb, Vector2 direction)
            {
                //throw new System.NotImplementedException();
            }
        }
    }
}
