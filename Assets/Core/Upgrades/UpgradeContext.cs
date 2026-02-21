using Core.Gameplay.Entity;
using Game.Entity.Player.Subsystem;

namespace Core.Upgrades
{
    public sealed class UpgradeContext
    {
        public readonly EntityController Entity;
        public readonly EntityModifierSubsystem Modifiers;
        
        public UpgradeContext(
            EntityController entity)
        {
            Entity = entity;
            Modifiers =
                entity.GetComponent<EntityModifierSubsystem>();
        }
    }
}
