using System.Collections.Generic;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.Modifiers;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Subsystem;
using Core.Gameplay.Entity.Tags;

namespace Core.Gameplay.Combat.StatusEffect.Implementations
{
    public class StatModifierEffectInstance : StatusEffectInstance
    {
        private readonly EntityController _target;
        private readonly StatusEffectData _data;
        private readonly GameplayTag _tag;

        private EntityModifierSubsystem _modifiers;
        // Stores the exact modifier values we registered so we can remove them.
        private readonly List<AttackStatModifier> _registeredModifiers = new();

        public StatModifierEffectInstance(
            EntityController target,
            StatusEffectData data)
        {
            _target = target;
            _data = data;
            SourceData = data;
            _tag = data.tagToApply;
            RemainingTime = data.duration;
        }

        public override void OnApply()
        {
            _modifiers = _target.GetComponent<EntityModifierSubsystem>();

            if (_modifiers == null)
                return;

            _registeredModifiers.Clear();

            foreach (var modifier in _data.statModifiers)
            {
                _modifiers.AddAttackStatModifier(modifier);
                _registeredModifiers.Add(modifier);     // track what we added
            }

            if (_tag != null)
                _target.Tags.AddTag(_tag);
        }

        public override void OnRemove()
        {
            if (_modifiers != null)
            {
                foreach (var handle in _registeredModifiers)
                    _modifiers.RemoveAttackStatModifier(handle);
            }

            _registeredModifiers.Clear();

            if (_tag != null)
                _target.Tags.RemoveTag(_tag);
        }
    }
}
