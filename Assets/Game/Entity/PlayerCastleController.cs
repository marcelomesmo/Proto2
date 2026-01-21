using System;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Spawn;
using Core.Gameplay.Entity.Tags;
using Core.Services;
using UnityEngine;

namespace Game.Entity
{
    /*
     * Entity responsible for:
        - Health scaling
        - Reward scaling
        - Meta progression hooks
        
        - Loot holder / loot magnetizer
     */
    public class PlayerCastleController : EntityController
    {
        public event Action CastleDestroyed;
        
        protected override void HandleTagAdded(GameplayTag tag)
        {
            // todo: remove this from here and add it to a entry-level sequencer later.
            if(tag == Stats.spawnFinishedTag)
                ServiceLocator.Get<GameController>().StartMatch();
            
            if (tag == Stats.deadTag)
                HandleDeath();
        }
        
        private void HandleDeath()
        {
            Debug.Log("[PlayerCastleController] Player is dead.");
            
            Tags.ClearTemporaryTags();
        }

        public override void NotifyDeathAnimationFinished()
        {
            // HARD INVARIANT: only valid if already dead
            if (!IsDead)
                return;
            
            // Signal death
            CastleDestroyed?.Invoke();
            
            Debug.Log("[PlayerCastleController] Player finished playing death animation.");
        }

        protected override void ApplySpawnScaling(in SpawnContext context)
        {
            // Example: scale HP, etc.
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
