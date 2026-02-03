using Core.Enum;
using Core.Gameplay.Entity;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public sealed class DamageSource
    {
        // Who caused it
        public readonly Faction faction;
        public readonly EntityController sourceEntity;
        
        // World position where the attack originated.
        // Not updated after execution.
        public readonly Vector2 sourcePosition;
        
        // Optional targeting rules (chain, AoE, friendly fire, etc.)
        public readonly AttackTargetFilter targetFilter;
        //public readonly Vector2 impactPosition; to add later.
        
        public DamageSource(
            Faction faction,
            EntityController sourceEntity,
            Vector2 sourcePosition,
            AttackTargetFilter targetFilter)
        {
            this.faction = faction;
            this.sourceEntity = sourceEntity;
            this.sourcePosition = sourcePosition;
            this.targetFilter = targetFilter;
        }
    }
}
