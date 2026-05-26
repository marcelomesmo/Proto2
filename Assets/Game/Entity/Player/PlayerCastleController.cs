using System;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Spawn;
using Core.Gameplay.Entity.Tags;
using UnityEngine;

namespace Game.Entity.Player
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
            if (tag == Stats.burnTag)
            {
                // do something? vfx
                //Debug.Log("[PlayerCastleController] Player is burning.");
            }
            
            if (tag == Stats.stunTag)
            {
                // do something? vfx
                //Debug.Log("[PlayerCastleController] Player is stunned.");
            }
            
            if (tag == Stats.slowTag)
            {
                // do something? vfx
                //Debug.Log("[PlayerCastleController] Player is slowed.");
            }
            
            if (tag == Stats.deadTag)
                HandleDeath();
        }
        
        private void HandleDeath()
        {
            //Debug.Log("[PlayerCastleController] Player is dead.");
            
            Tags.ClearTemporaryTags();
        }

        public override void NotifyDeathAnimationFinished()
        {
            // HARD INVARIANT: only valid if already dead
            if (!IsDead)
                return;
            
            // Signal death
            CastleDestroyed?.Invoke();
            
            //Debug.Log("[PlayerCastleController] PlayerCastle finished playing death animation.");
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
