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
        
        public override void MoveTo(Vector2 target)
        {
            if (!CanRbMove)  // Important to check if rb.simulated is true before moving.
                return;
            
            // Only move along X
            float dx = target.x - Rb.position.x;

            // Already close enough
            if (Mathf.Abs(dx) < 0.01f)
            {
                Stop();
                return;
            }

            float directionX = Mathf.Sign(dx);

            float speed =
                Controller.Stats.moveSpeed * SpeedMultiplier;

            Rb.linearVelocity = new Vector2(directionX * speed, 0f);

            // Hard-lock Y to lane to avoid drift
            Rb.position = new Vector2(Rb.position.x, _laneY);

            Vector2 toTarget = (target - Rb.position).normalized;

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

            // Ensure we never accumulate Y drift
            Rb.linearVelocity = new Vector2(0f, 0f);
            Rb.position = new Vector2(Rb.position.x, _laneY);
        }
    }
}
