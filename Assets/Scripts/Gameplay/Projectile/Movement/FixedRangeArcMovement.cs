using UnityEngine;

[CreateAssetMenu(fileName = "FixedRangeArcMovement", menuName = "Projectile/Movement/Fixed Range Arc")]
public class FixedRangeArcMovement : ProjectileMovement
{
    // This projectile is defined by a Launch Angle, Speed and Final destination.
    // Range is more reliable than CannonBall. Increases bullet speed with higher angles.
    [Header("Arc Settings")]
    public float launchAngleDeg = 15f;      // arc height control (0–60)
    
    private void OnEnable()
    {
        physicsMode = ProjectilePhysicsMode.Dynamic;
    }
    
    public override MovementContext CreateContext()
    {
        return new FixedRangeArcMovementContext(launchAngleDeg);
    }

    private class FixedRangeArcMovementContext : MovementContext
    {
        private readonly float _launchAngleDeg;
        
        public FixedRangeArcMovementContext(float launchAngleDeg)
        {
            _launchAngleDeg = launchAngleDeg;
        }
        
        public override void Initialize(Rigidbody2D rb, Vector2 direction, float range)
        {
            // Range - distance to landing point
            // Launch Angle Deg - arc height control (0-60) degrees
            // Speed - controls total travel time
            
            rb.gravityScale = 1f; // must be enabled
        
            // Normalize direction from shooter -> forward
            direction.Normalize();
        
            // Compute required initial velocity for 2D case:
            // p(t) = p0 + v0 t + ½ g t², into
            // v0 = (p(t) - p0 - ½ g t²) / t
            // where pt - p0 = d, 
            // v0 = (d - 0.5 * g * t²) / t
        
            // gravity magnitude (positive)
            float g = Mathf.Abs(Physics2D.gravity.y);
        
            // angle in radians and sanity checks
            float angle = _launchAngleDeg * Mathf.Deg2Rad;

            // sin(2θ) must be > 0 to have a valid positive range for v
            float sin2t = Mathf.Sin(2f * angle);
            if (Mathf.Abs(sin2t) < 1e-5f)
            {
                // angle too shallow/too vertical -> fallback to small angle
                angle = Mathf.Clamp(angle, 5f * Mathf.Deg2Rad, 85f * Mathf.Deg2Rad);
                sin2t = Mathf.Sin(2f * angle);
            }
        
            // Compute required speed magnitude
            // v^2 = R * g / sin(2θ)
            float v2 = (range * g) / sin2t;
            if (v2 < 0f) v2 = 0f;
            float v = Mathf.Sqrt(v2);
        
            // Build velocity vector in 2D: horizontal component uses signX
            float vx = v * Mathf.Cos(angle) * direction.x;
            float vy = v * Mathf.Sin(angle);

            // Set velocity ONCE — after this physics takes over.
            rb.linearVelocity = new Vector2(vx, vy);
        }

        public override void Move(Rigidbody2D rb, Vector2 direction)
        {
            // Do nothing — ballistic projectiles should not be modified per frame.
            // Physics handles trajectory.
        }
    }
}
