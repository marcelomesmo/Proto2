using Core.Enum;
using Core.Gameplay.Entity;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public sealed class DamageSource
    {
        // Who caused it
        public readonly Faction faction;
        public readonly EntityController controller;
        // World position where the attack originated.
        // Not updated after execution.
        public readonly Vector2 sourcePosition;
        //public readonly Vector2 impactPosition; to add later.
        public readonly AttackTargetFilter targetFilter;
        
        public DamageSource(
            Faction faction,
            EntityController controller,
            Vector2 sourcePosition,
            AttackTargetFilter targetFilter)
        {
            this.faction = faction;
            this.controller = controller;
            this.sourcePosition = sourcePosition;
            this.targetFilter = targetFilter;
        }
    }
}
