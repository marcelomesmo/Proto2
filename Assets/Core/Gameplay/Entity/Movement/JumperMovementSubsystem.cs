using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Core.Gameplay.Entity.Movement
{
    public class JumperMovementSubsystem : EntityMovement
    {
        public override void MoveTo(Vector2 target)
        {
            /*if(!CanRbMove) return;
        
            Vector2 direction = (target - (Vector2)transform.position).normalized;
            float jumpVelocityY = Mathf.Sqrt(2f * Controller.Stats.jumpHeight * Mathf.Abs(Physics2D.gravity.y));
            Rb.linearVelocity = new Vector2(direction.x * Controller.Stats.moveSpeed, jumpVelocityY);

            CheckDirectionChange(direction);
            
            // Play jump animation
            if (Controller.Animator)
                Controller.Animator.SetTrigger("jump");*/
        }
    }
}
