using Entity;
using UnityEngine;

namespace Gameplay.Projectile
{
    public readonly struct DamageSource
    {
        // Who caused it
        public readonly Faction faction;
        public readonly EntityController controller;
        public readonly Vector2 sourcePosition;
        
        public DamageSource(
            Faction faction,
            EntityController controller,
            Vector2 sourcePosition)
        {
            this.faction = faction;
            this.controller = controller;
            this.sourcePosition = sourcePosition;
        }
    }
}
