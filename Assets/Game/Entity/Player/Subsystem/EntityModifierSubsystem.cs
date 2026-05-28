using System.Collections.Generic;
using Core.Gameplay.Combat.Modifiers;
using Core.Gameplay.Entity.Subsystem;
using Core.Upgrades.Effects;

namespace Game.Entity.Player.Subsystem
{
    // TODO: Move this to Core since all the modifiers are there. Each specific game will only uses the ones they need.
    // (deprecated: Make base EntityModifierSubsystem in Core and extend it to add xp only?)
    public sealed class EntityModifierSubsystem : BaseSubsystem
    {
        private readonly List<DamageModifier> _damageModifiers = new();
        private readonly List<HealthModifier> _healthModifiers = new();
        private readonly List<ExperienceModifier> _xpModifiers = new();

        // ----------------
        //  Lifecycle flow
        // ----------------
        protected override void OnInitialize()
        {
            _damageModifiers.Clear();
            _healthModifiers.Clear();
            _xpModifiers.Clear();
        }

        protected override void OnDeinitialize()
        {
            _damageModifiers.Clear();
            _healthModifiers.Clear();
            _xpModifiers.Clear();
        }

        // ----------------
        // Damage
        // ----------------

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
       
        // ----------------
        // Health
        // ----------------
        
        public void AddHealthModifier(HealthModifier modifier)
        {
            _healthModifiers.Add(modifier);
        }

        public void RemoveHealthModifier(HealthModifier modifier)
        {
            _healthModifiers.Remove(modifier);
        }

        public IReadOnlyList<HealthModifier> HealthModifiers =>
            _healthModifiers;

        // ----------------
        // XP
        // ----------------

        public void AddXpModifier(ExperienceModifier modifier)
        {
            _xpModifiers.Add(modifier);
        }

        public void RemoveXpModifier(ExperienceModifier modifier)
        {
            _xpModifiers.Remove(modifier);
        }
        
        public IReadOnlyList<ExperienceModifier> XpModifiers =>
            _xpModifiers;
    }
}
