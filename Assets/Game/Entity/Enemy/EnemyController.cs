using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Spawn;
using Core.Gameplay.Entity.Tags;
using Core.Services;
using Core.Services.Manager;
using UnityEngine;

namespace Game.Entity.Enemy
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
            
            // This is now a valid moment to:
            // - Notifies a wave manager
            // - Signals an encounter controller
            // - Informs a boss phase system
            // - Coordinates multiple entities
            
            ServiceLocator
                .Get<GameController>()
                .MatchStats
                .RegisterEnemyKilled();
            
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
        
        protected override void ApplySpawnScaling(in SpawnContext context)
        {
            // Example: scale HP, attack, etc.
            Stats.maxHealth = Mathf.RoundToInt(
                Stats.maxHealth * context.PowerMultiplier
            );
            
            // Future:
            // - level-based curves
            // - rarity multipliers
            // - idle RPG prestige bonuses
        }
    }
}