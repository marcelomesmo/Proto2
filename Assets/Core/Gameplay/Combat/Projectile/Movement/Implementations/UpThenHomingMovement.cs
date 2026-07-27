using Core.Gameplay.Entity;
using UnityEngine;

namespace Core.Gameplay.Combat.Projectile.Movement.Implementations
{
    [CreateAssetMenu(
        fileName = "UpThenHomingMovement",
        menuName = "Combat/Projectile/Movement/Up Then Homing")]
    public sealed class UpThenHomingMovement : ProjectileMovement
    {
        [Header("Rise Phase")]
        [SerializeField] private float riseDuration = 0.18f;
        [SerializeField] private float riseSpeed = 8f;
        [SerializeField] private float forwardSpeedDuringRise = 1.5f;

        [Header("Homing Phase")]
        [SerializeField] private float turnDegreesPerSecond = 1440f;
        [SerializeField] private bool snapDirectionAtApex = true;
        [SerializeField] private float arrivalSnapDistance = 0.05f;

        [Header("Targeting")]
        [SerializeField] private Vector2 targetOffset;

        private void OnEnable()
        {
            // This movement is scripted, not ballistic.
            // If your enum does not have Kinematic, use Dynamic but keep gravityScale = 0
            // and drive the body manually.
            physicsMode = ProjectilePhysicsMode.Kinematic;
        }

        public override MovementContext CreateContext()
        {
            return new UpThenHomingMovementContext(
                speed,
                riseDuration,
                riseSpeed,
                forwardSpeedDuringRise,
                turnDegreesPerSecond,
                snapDirectionAtApex,
                arrivalSnapDistance,
                targetOffset);
        }

        private sealed class UpThenHomingMovementContext :
            MovementContext,
            IProjectileTargetReceiver
        {
            private enum Phase
            {
                Rise,
                Home,
                FlyForward
            }

            private readonly float _homeSpeed;
            private readonly float _riseDuration;
            private readonly float _riseSpeed;
            private readonly float _forwardSpeedDuringRise;
            private readonly float _turnDegreesPerSecond;
            private readonly bool _snapDirectionAtApex;
            private readonly float _arrivalSnapDistance;
            private readonly Vector2 _targetOffset;

            private Transform _target;
            private Phase _phase;
            private float _elapsed;

            private Vector2 _initialForward;
            private Vector2 _currentDirection;
            private EntityController _targetEntity;
            private Vector2 _lastKnownTargetPosition;
            private bool _hasLastKnownTargetPosition;

            public UpThenHomingMovementContext(
                float homeSpeed,
                float riseDuration,
                float riseSpeed,
                float forwardSpeedDuringRise,
                float turnDegreesPerSecond,
                bool snapDirectionAtApex,
                float arrivalSnapDistance,
                Vector2 targetOffset)
            {
                _homeSpeed = homeSpeed;
                _riseDuration = Mathf.Max(0.01f, riseDuration);
                _riseSpeed = riseSpeed;
                _forwardSpeedDuringRise = forwardSpeedDuringRise;
                _turnDegreesPerSecond = turnDegreesPerSecond;
                _snapDirectionAtApex = snapDirectionAtApex;
                _arrivalSnapDistance = Mathf.Max(0f, arrivalSnapDistance);
                _targetOffset = targetOffset;
            }

            public void SetTarget(Transform target)
            {
                _target = target;

                if (_target != null)
                {
                    _lastKnownTargetPosition = GetTargetPosition();
                    _hasLastKnownTargetPosition = true;
                }
                
                _targetEntity = target
                    ? target.GetComponentInParent<EntityController>()
                    : null;
            }

            public override void Initialize(
                Rigidbody2D rb,
                Vector2 direction,
                float range)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;

                _phase = Phase.Rise;
                _elapsed = 0f;

                _initialForward = GetInitialForward(direction);

                Vector2 riseVelocity =
                    Vector2.up * _riseSpeed +
                    _initialForward * _forwardSpeedDuringRise;

                _currentDirection = riseVelocity.sqrMagnitude > 0.0001f
                    ? riseVelocity.normalized
                    : Vector2.up;

                RotateToDirection(rb, _currentDirection);
            }

            public override void Move(Rigidbody2D rb, Vector2 direction)
            {
                float dt = Time.fixedDeltaTime;

                switch (_phase)
                {
                    case Phase.Rise:
                        MoveRise(rb, dt);
                        break;

                    case Phase.Home:
                        MoveHome(rb, direction, dt);
                        break;
                    
                    case Phase.FlyForward:
                        MoveForward(rb, direction, dt);
                        break;
                }
            }

            private void MoveRise(Rigidbody2D rb, float dt)
            {
                _elapsed += dt;

                Vector2 velocity =
                    Vector2.up * _riseSpeed +
                    _initialForward * _forwardSpeedDuringRise;

                Vector2 nextPosition = rb.position + velocity * dt;

                rb.MovePosition(nextPosition);

                if (velocity.sqrMagnitude > 0.0001f)
                {
                    _currentDirection = velocity.normalized;
                    RotateToDirection(rb, _currentDirection);
                }

                if (_elapsed >= _riseDuration)
                {
                    _phase = Phase.Home;

                    if (_snapDirectionAtApex && TryGetDesiredDirection(rb, Vector2.zero, out Vector2 desiredDirection))
                    {
                        _currentDirection = desiredDirection;
                        RotateToDirection(rb, _currentDirection);
                    }
                }
            }

            private void MoveHome(Rigidbody2D rb, Vector2 fallbackDirection, float dt)
            {
                // Once the target becomes invalid, stop homing permanently.
                if (!TryGetTargetPosition(rb, fallbackDirection, out Vector2 targetPosition))
                {
                    _phase = Phase.FlyForward;

                    MoveForward(
                        rb,
                        fallbackDirection,
                        dt);

                    return;
                }

                Vector2 toTarget = targetPosition - rb.position;
                float distance = toTarget.magnitude;

                // The projectile has reached the target's current or
                // last-known position without producing an impact.
                if (distance <= 0.0001f)
                {
                    // Do not remain parked here.
                    _phase = Phase.FlyForward;
                    
                    MoveForward(
                        rb,
                        fallbackDirection,
                        dt);
                    
                    //rb.MovePosition(targetPosition);
                    return;
                }

                Vector2 desiredDirection = toTarget / distance;

                if (_currentDirection.sqrMagnitude <= 0.0001f)
                {
                    _currentDirection = desiredDirection;
                }
                else
                {
                    float maxRadiansDelta =
                        _turnDegreesPerSecond *
                        Mathf.Deg2Rad *
                        dt;

                    _currentDirection = Vector3
                        .RotateTowards(
                            _currentDirection,
                            desiredDirection,
                            maxRadiansDelta,
                            0f)
                        .normalized;
                }

                float stepDistance = _homeSpeed * dt;

                // Prevent visual overshoot/orbiting around the target point.
                if (distance <= stepDistance + _arrivalSnapDistance)
                {
                    rb.MovePosition(targetPosition);
                    
                    // Preserve the final direction of the homing arc.
                    // The projectile will continue along this direction.
                    _currentDirection = desiredDirection;

                    RotateToDirection(rb, _currentDirection);
                    
                    // Normally, the target collider should process the impact during
                    // this physics step. If it does not, continue forward next tick
                    // instead of remaining stationary.
                    _phase = Phase.FlyForward;
                    
                    return;
                }

                Vector2 nextPosition =
                    rb.position +
                    _currentDirection * stepDistance;

                rb.MovePosition(nextPosition);
                RotateToDirection(rb, _currentDirection);
            }

            private void MoveForward(
                Rigidbody2D rb,
                Vector2 fallbackDirection,
                float dt)
            {
                Vector2 travelDirection =
                    _currentDirection;

                if (travelDirection.sqrMagnitude <= 0.0001f)
                {
                    travelDirection =
                        fallbackDirection.sqrMagnitude > 0.0001f
                            ? fallbackDirection.normalized
                            : _initialForward;
                }

                if (travelDirection.sqrMagnitude <= 0.0001f)
                    return;

                _currentDirection =
                    travelDirection.normalized;

                Vector2 nextPosition =
                    rb.position +
                    _currentDirection * (_homeSpeed * dt);

                rb.MovePosition(nextPosition);

                RotateToDirection(
                    rb,
                    _currentDirection);
            }
            
            private bool TryGetDesiredDirection(
                Rigidbody2D rb,
                Vector2 fallbackDirection,
                out Vector2 desiredDirection)
            {
                desiredDirection = Vector2.zero;

                if (!TryGetTargetPosition(rb, fallbackDirection, out Vector2 targetPosition))
                    return false;

                Vector2 toTarget = targetPosition - rb.position;

                if (toTarget.sqrMagnitude <= 0.0001f)
                    return false;

                desiredDirection = toTarget.normalized;
                return true;
            }

            private bool TryGetTargetPosition(
                Rigidbody2D rb,
                Vector2 fallbackDirection,
                out Vector2 targetPosition)
            {
                if (HasLiveTarget())
                {
                    targetPosition = GetTargetPosition();
                    
                    _lastKnownTargetPosition = targetPosition;
                    _hasLastKnownTargetPosition = true;
                    
                    return true;
                }

                // The target died or disappeared.
                // Finish the arc toward its last valid position.
                if (_hasLastKnownTargetPosition)
                {
                    targetPosition = _lastKnownTargetPosition;
                    return true;
                }

                // No target position was ever available.
                if (fallbackDirection.sqrMagnitude > 0.0001f)
                {
                    targetPosition = rb.position + fallbackDirection.normalized * 100f;
                    return true;
                }

                targetPosition = rb.position;
                return false;
            }

            private Vector2 GetTargetPosition()
            {
                return (Vector2)_target.position + _targetOffset;
            }
            
            private bool HasLiveTarget()
            {
                /*
                 * This handles all three relevant cases:

                    Destroyed target.
                    Pooled or deactivated target.
                    Dead target that remains active for its death animation.
                 */
                
                if (!_target)
                    return false;

                if (!_target.gameObject.activeInHierarchy)
                    return false;

                if (_targetEntity && _targetEntity.IsDead)
                    return false;

                return true;
            }

            private static Vector2 GetInitialForward(Vector2 direction)
            {
                if (Mathf.Abs(direction.x) > 0.001f)
                    return new Vector2(Mathf.Sign(direction.x), 0f);

                if (direction.sqrMagnitude > 0.0001f)
                    return direction.normalized;

                return Vector2.right;
            }

            private static void RotateToDirection(Rigidbody2D rb, Vector2 direction)
            {
                if (direction.sqrMagnitude <= 0.0001f)
                    return;

                float angle =
                    Mathf.Atan2(direction.y, direction.x) *
                    Mathf.Rad2Deg;

                rb.rotation = angle;
            }
        }
    }
}