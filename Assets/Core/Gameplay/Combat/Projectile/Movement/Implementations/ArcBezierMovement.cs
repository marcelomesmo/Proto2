using UnityEngine;

namespace Core.Gameplay.Combat.Projectile.Movement.Implementations
{
    [CreateAssetMenu(fileName = "ArcBezierMovement", menuName = "Projectile/Movement/Arc Bezier")]
    public class ArcBezierMovement : ProjectileMovement
    {
        public float travelTime = 0.5f;   // <- This will now be ALWAYS the same
        public float arcHeight = 1.5f;    // <- Controls arc shape
    
        private void OnEnable()
        {
            physicsMode = ProjectilePhysicsMode.Kinematic;
        }
    
        public override MovementContext CreateContext()
        {
            return new ArcBezierMovementContext(travelTime, arcHeight);
        }
    
        // Per-instance context (runtime state)
        private class ArcBezierMovementContext : MovementContext
        {
            private readonly float _travelTime;
            private readonly float _arcHeight;

            private Vector2 p0, p1, p2;
            private float timer;
            private bool finished;

            public ArcBezierMovementContext(float travelTime, float arcHeight)
            {
                _travelTime = Mathf.Max(0.0001f, travelTime);
                _arcHeight = arcHeight;
                timer = 0f;
                finished = false;
            }

            public override void Initialize(Rigidbody2D rb, Vector2 direction, float range)
            {
                direction.Normalize();

                // Start and end positions (respect the provided range)
                p0 = rb.position;
                p2 = p0 + direction * range;

                // middle control point lifted by arcHeight relative to shot
                Vector2 mid = (p0 + p2) * 0.5f;
                Vector2 up = Vector2.Perpendicular(direction).normalized; // perpendicular to forward
                p1 = mid + up * _arcHeight;

                timer = 0f;
                finished = false;
            }

            public override void Move(Rigidbody2D rb, Vector2 direction)
            {
                if (finished) return;

                timer += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(timer / _travelTime);

                // Quadratic Bezier formula
                Vector2 pos =
                    (1 - t) * (1 - t) * p0 +
                    2f * (1 - t) * t * p1 +
                    t * t * p2;

                rb.MovePosition(pos);

                if (t >= 1f)
                    finished = true;
            }
        }
    
/*
    public override void Initialize(Rigidbody2D rb, Vector2 direction)
    {
        direction.Normalize();

        p0 = rb.position;

        // End point
        p2 = p0 + direction * fixedRange;

        // Middle control point — lifted by arcHeight
        Vector2 mid = (p0 + p2) * 0.5f;
        Vector2 up = Vector2.Perpendicular(direction).normalized; // upward relative to shot
        p1 = mid + up * arcHeight;

        timer = 0f;

        //rb.bodyType = RigidbodyType2D.Kinematic; // IMPORTANT for kinematic movement
    }

    public override void Move(Rigidbody2D rb, Vector2 direction)
    {
        if (timer > travelTime)
            return;

        timer += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(timer / travelTime);

        // Quadratic Bezier
        Vector2 pos =
            (1 - t) * (1 - t) * p0 +
            2 * (1 - t) * t * p1 +
            t * t * p2;

        rb.MovePosition(pos);
    }*/
    }
}
