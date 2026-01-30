using Core.EventChannels;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Spawn;
using Core.Gameplay.Entity.Tags;
using Core.Services;
using Core.Services.Manager;
using Game.Entity.Enemy.Stats;
using Game.Services.Meta;
using UnityEngine;

namespace Game.Entity.Enemy
{
    public class EnemyController : EntityController
    {
        [Header("Broadcast")]
        [SerializeField] private IntEventChannelSO xpEvent;
        
        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == Stats.deadTag)
                OnDeath();
            
            if (tag == Stats.burnTag)
            {
                // do something? vfx
                //Debug.Log("[EnemyController] is burning.");
            }
            
            if (tag == Stats.stunTag)
            {
                // do something? vfx
                //Debug.Log("[EnemyController] is stunned.");
            }
            
            if (tag == Stats.slowTag)
            {
                // do something? vfx
                //Debug.Log("[EnemyController] is slowed.");
            }
        }

        private void OnDeath()
        {
            //Debug.Log("[BaseEnemyController] Enemy died.");
            
            // This is now a valid moment to:
            // - Notifies a wave manager
            // - Signals an encounter controller
            // - Informs a boss phase system
            // - Coordinates multiple entities

            var stats = Stats as EnemyStats;
            if (stats)
                xpEvent.RaiseEvent(stats.xpReward);

            var gameStats = ServiceLocator
                .Get<GameController>()
                .MatchStats as GameMatchStats;

            gameStats?.RegisterEnemyKilled();
            
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