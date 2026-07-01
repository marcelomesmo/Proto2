using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Core.Gameplay.Entity.Movement
{
    public class LaneMovementSubsystem : EntityMovement
    {
        private float _laneY;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();

            // Lock the lane on spawn
            _laneY = Rb.position.y;

            // Disable physics features we don't want
            Rb.gravityScale = 0f;
            Rb.freezeRotation = true;
        }
        
        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            if (!CanRbMove)
                return;

            // Hard-lock Y to lane after velocity has been applied.
            Rb.position = new Vector2(Rb.position.x, _laneY);
        }
        
        public override void MoveTo(Vector2 target)
        {
            if (!CanRbMove)  // Important to check if rb.simulated is true before moving.
                return;
            
            // Knockback has priority over normal movement.
            if (IsKnockbackActive)
            {
                SetLocomotionVelocity(Vector2.zero);
                return;
            }
            
            // Only move along X
            float dx = target.x - Rb.position.x;

            // Already close enough
            if (Mathf.Abs(dx) < 0.01f)
            {
                Stop();
                return;
            }

            float directionX = Mathf.Sign(dx);
            float speed = GetEffectiveMoveSpeed();

            SetLocomotionVelocity(new Vector2(directionX * speed, 0f));

            if (!Controller.Animator)
                return;

            Controller.Animator.SetBool("isMoving", true);
            Controller.Animator.SetFloat(
                "velocityX",
                Mathf.Abs(Rb.linearVelocity.x) / Controller.Stats.moveSpeed
            );
        }
        
        public override void Stop()
        {
            base.Stop();
            
            Rb.position = new Vector2(Rb.position.x, _laneY);
        }
        
        protected override Vector2 ConstrainLocomotionVelocity(Vector2 velocity)
        {
            return new Vector2(velocity.x, 0f);
        }

        protected override Vector2 ConstrainFinalVelocity(Vector2 velocity)
        {
            return new Vector2(velocity.x, 0f);
        }

        protected override Vector2 ConstrainKnockbackVelocity(Vector2 velocity)
        {
            if (Mathf.Abs(velocity.x) < 0.001f)
                return Vector2.zero;

            // Preserve knockback magnitude, but force it onto the lane axis.
            return new Vector2(Mathf.Sign(velocity.x) * velocity.magnitude, 0f);
        }
    }
}
