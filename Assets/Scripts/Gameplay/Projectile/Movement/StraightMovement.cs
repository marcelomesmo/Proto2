using UnityEngine;

[CreateAssetMenu(fileName = "StraightMovement", menuName = "Projectile/Movement/Straight")]
public class StraightMovement : ProjectileMovement
{
    private void OnEnable()
    {
        physicsMode = ProjectilePhysicsMode.Dynamic;
    }
    
    public override MovementContext CreateContext()
    {
        return new StraightMovementContext(speed);
    }

    private class StraightMovementContext : MovementContext
    {
        readonly float speed;
        
        public StraightMovementContext(float speed)
        {
            this.speed = speed;
        }
        
        public override void Initialize(Rigidbody2D rb, Vector2 direction, float range)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = direction.normalized * speed;
        }

        public override void Move(Rigidbody2D rb, Vector2 direction)
        {
            // If you want constant velocity, you don't need to update every FixedUpdate.
            // But you could if you want to reapply or modulate velocity.
            // e.g. rb.linearVelocity = direction.normalized * speed * falldown;
        }
    }
}
