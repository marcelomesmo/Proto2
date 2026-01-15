using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Tags;
using Core.Services.Manager;
using Entity;
using UnityEngine;
using UnityEngine.Pool;

namespace Enemy
{
    public class EnemyController : EntityController
    {
        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == Stats.deadTag)
                OnDeath();
        }

        private void OnDeath()
        {
            //Debug.Log("[BaseEnemyController] Enemy died.");
            
            // This is now only valid if Controller:
            // - Notifies a wave manager
            // - Signals an encounter controller
            // - Informs a boss phase system
            // - Coordinates multiple entities
            
            Tags.ClearTemporaryTags();
        }

        public override void NotifyDeathAnimationFinished()
        {
            // HARD INVARIANT: only valid if already dead
            if (!IsDead)
                return;
            
            if (IsReleased)
                return;

            EntityPoolManager.Instance.Despawn(this);
        }
    }
}