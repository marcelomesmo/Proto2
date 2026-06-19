using System.Collections.Generic;
using Core.Gameplay.Combat.Modifiers;
using Core.Upgrades.Effects;

namespace Core.Gameplay.Entity.Subsystem
{
    public sealed class EntityModifierSubsystem : BaseSubsystem
    {
        private readonly List<AttackStatModifier> _attackStatModifiers = new();
        private readonly List<AttackVariantModifier> _attackVariantModifiers = new();
        private readonly List<HealthModifier> _healthModifiers = new();
        private readonly List<ExperienceModifier> _xpModifiers = new();

        // ----------------
        //  Lifecycle flow
        // ----------------
        protected override void OnInitialize()
        {
            _attackStatModifiers.Clear();
            _attackVariantModifiers.Clear();
            _healthModifiers.Clear();
            _xpModifiers.Clear();
        }

        protected override void OnDeinitialize()
        {
            _attackStatModifiers.Clear();
            _attackVariantModifiers.Clear();
            _healthModifiers.Clear();
            _xpModifiers.Clear();
        }
        
        // ----------------
        // Attack Stats (Damage, Healing, Range, Duration, Cooldown)
        // ----------------
        
        public void AddAttackStatModifier(AttackStatModifier modifier)
        {
            _attackStatModifiers.Add(modifier);
        }

        public void RemoveAttackStatModifier(AttackStatModifier modifier)
        {
            _attackStatModifiers.Remove(modifier);
        }
        
        public IReadOnlyList<AttackStatModifier> AttackStatModifiers =>
            _attackStatModifiers;
        
        // ----------------
        // Attack Variants (AttackData replacement)
        // ----------------
        
        public void AddAttackVariantModifier(AttackVariantModifier modifier)
        {
            _attackVariantModifiers.Add(modifier);
        }

        public void RemoveAttackVariantModifier(AttackVariantModifier modifier)
        {
            _attackVariantModifiers.Remove(modifier);
        }
        
        public IReadOnlyList<AttackVariantModifier> AttackVariantModifiers =>
            _attackVariantModifiers;
        
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
