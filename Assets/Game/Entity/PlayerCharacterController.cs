using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Spawn;
using Core.Gameplay.Entity.Tags;
using UnityEngine;

namespace Game.Entity
{
    public class PlayerCharacterController : EntityController
    {
        /*protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == Stats.deadTag)
                HandleDeath();
        }
        
        private void HandleDeath()
        {
            Tags.ClearTemporaryTags();
        }*/

        public override void NotifyDeathAnimationFinished()
        {
            // HARD INVARIANT: only valid if already dead
            if (!IsDead)
                return;
            
            Debug.Log("[CharacterController] Character " + Stats.name + " is dead.");
            
            //EntityPoolManager.Instance.Despawn(this);
        }

        protected override void ApplySpawnScaling(in SpawnContext context)
        {
            // Example: scale attack, speed, etc.
            
            // Future:
            // - level-based curves
            // - rarity multipliers
            // - idle RPG prestige bonuses
        }
    }
}