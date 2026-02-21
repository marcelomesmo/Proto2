using System.Collections.Generic;
using Core.Gameplay.Combat.Modifiers;
using Core.Gameplay.Entity.Subsystem;
using Core.Upgrades.Effects;

namespace Game.Entity.Player.Subsystem
{
    // TODO: Make base EntityModifierSubsystem in Core and extend it to add xp only.
    public sealed class EntityModifierSubsystem : BaseSubsystem
    {
        private readonly List<DamageModifier> _damageModifiers = new();
        private readonly List<ExperienceModifier> _xpModifiers = new();

        // ---------------- Damage ----------------

        public void AddDamageModifier(DamageModifier modifier)
        {
            _damageModifiers.Add(modifier);
        }

        public void RemoveDamageModifier(DamageModifier modifier)
        {
            _damageModifiers.Remove(modifier);
        }

        public IReadOnlyList<DamageModifier> DamageModifiers =>
            _damageModifiers;

        // ---------------- XP ----------------

        public void AddXpModifier(ExperienceModifier modifier)
        {
            _xpModifiers.Add(modifier);
        }

        public void RemoveXpModifier(ExperienceModifier modifier)
        {
            _xpModifiers.Remove(modifier);
        }

        public float GetXpMultiplier()
        {
            float result = 1f;

            foreach (var mod in _xpModifiers)
                result *= mod.multiplier;

            return result;
        }
    }
}
