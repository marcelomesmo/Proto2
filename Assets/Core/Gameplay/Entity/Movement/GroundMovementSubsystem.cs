using Core.Gameplay.Entity.Subsystem;
using Entity;
using UnityEngine;

namespace Core.Gameplay.Entity.Movement
{
    public class GroundMovementSubsystem : EntityMovement
    {
        public override void MoveTo(Vector2 target)
        {
            if (!CanRbMove)  // Important to check if rb.simulated is true before moving.
                return;

            Vector2 targetPos = new Vector2(target.x, transform.position.y);
       
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;

            Rb.linearVelocity = new Vector2(direction.x * Controller.Stats.moveSpeed, Rb.linearVelocity.y);

            CheckDirectionChange(direction);
        
            // Animation triggers
            if (!Controller.Animator)
                return;
        
            Controller.Animator.SetBool("isMoving", true);
            Controller.Animator.SetFloat("velocityX", Mathf.Abs(Rb.linearVelocity.x) / Controller.Stats.moveSpeed);
        }
    }
}
