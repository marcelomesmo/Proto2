using Core.Gameplay.Entity.Subsystem;
using Game.Entity.Enemy.Stats;
using UnityEngine;

namespace Core.Gameplay.Entity.Movement
{
    
    // DEPRECATED
    public class FlyMovementSubsystem : EntityMovement
    {
        private EnemyStats _stats;
    
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            Rb.gravityScale = 0f;
            
            if (Controller.Stats is EnemyStats enemyStats)
                _stats = enemyStats;
            else
                Debug.LogError("[DasherMovementSubsystem] requires EnemyStats!");
        }
        
        public override void MoveTo(Vector2 target)
        {
            if (!CanRbMove)  // Important to check if rb.simulated is true before moving.
                return;

            Vector2 targetPos = new Vector2(target.x, target.y); //+ _stats.flyHoverHeight); todo
       
            Vector2 direction = (targetPos - (Vector2)transform.position).normalized;

            Rb.linearVelocity = direction * Controller.Stats.moveSpeed;
        
            // Animation triggers
            if (!Controller.Animator)
                return;
        
            Controller.Animator.SetBool("isFlying", true);
            Controller.Animator.SetFloat("velocityX", Mathf.Abs(Rb.linearVelocity.x) / Controller.Stats.moveSpeed);
        }
    }
}
