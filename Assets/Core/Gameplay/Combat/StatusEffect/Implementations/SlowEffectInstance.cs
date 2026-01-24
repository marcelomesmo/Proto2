using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Subsystem;
using Core.Gameplay.Entity.Tags;

namespace Core.Gameplay.Combat.StatusEffect.Implementations
{
    public class SlowEffectInstance : StatusEffectInstance
    {
        private readonly EntityController _target;
        private readonly float _multiplier;
        private readonly GameplayTag _slowTag;

        public SlowEffectInstance(
            EntityController target,
            AttackEffectData data,
            GameplayTag slowTag)
        {
            _target = target;
            _multiplier = data.value;
            _slowTag = slowTag;
            RemainingTime = data.duration;
        }

        public override void OnApply()
        {
            _target.Tags.AddTag(_slowTag);
            
            if (_target.TryGetComponent(out EntityMovement movement))
                movement.SetSpeedMultiplier(_multiplier);
        }

        public override void OnRemove()
        {
            _target.Tags.RemoveTag(_slowTag);
            
            if (_target.TryGetComponent(out EntityMovement movement))
                movement.SetSpeedMultiplier(1f);
            
            // todo: multiple slow effects dont' overlap or clean each other, improve this later
        }
    }
}
