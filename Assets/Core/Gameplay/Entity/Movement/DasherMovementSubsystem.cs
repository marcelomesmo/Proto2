using Core.Gameplay.Entity.Subsystem;
using Game.Entity.Enemy.Stats;
using UnityEngine;

namespace Core.Gameplay.Entity.Movement
{
    // DEPRECATED
    public class DasherMovementSubsystem : EntityMovement
    {
        private EnemyStats _stats;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            if (Controller.Stats is EnemyStats enemyStats)
                _stats = enemyStats;
            else
                Debug.LogError("[DasherMovementSubsystem] requires EnemyStats!");
        }
        
        public override void MoveTo(Vector2 target)
        {
            if(!CanRbMove) return;
            
            var direction = (target - (Vector2)transform.position).normalized;
            Rb.linearVelocity = direction * (Controller.Stats.moveSpeed);//* _stats.dashSpeedMultiplier); todo
            
            // Play dash animation
            if (Controller.Animator)
                Controller.Animator.SetTrigger("dash");
        }
    }
}
