using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Tags;

namespace Core.Gameplay.Combat.StatusEffect.Implementations
{
    public class StunEffectInstance : StatusEffectInstance
    {
        private readonly EntityController _target;
        private readonly GameplayTag _stunTag;

        public StunEffectInstance(
            EntityController target,
            StatusEffectData data)
        {
            _target = target;
            SourceData = data;
            _stunTag = data.tagToApply;
            RemainingTime = data.duration;
        }

        public override void OnApply()
        {
            _target.Tags.AddTag(_stunTag);
        }

        public override void OnRemove()
        {
            _target.Tags.RemoveTag(_stunTag);
        }
    }
}
