using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Subsystem;

namespace Core.Gameplay.Combat.Attack
{
    public struct AttackResolveContext
    {
        public EntityController Source;
        public EntityModifierSubsystem Modifiers;
    }
}
