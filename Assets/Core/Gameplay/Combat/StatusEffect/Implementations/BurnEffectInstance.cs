using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Tags;
using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Combat.StatusEffect.Implementations
{
    public class BurnEffectInstance : StatusEffectInstance
    {
        private readonly EntityController _target;
        private readonly AttackEffectData _data;
        private readonly AttackSource _attackSource;
        private float _tickTimer;
        
        private readonly GameplayTag _burnTag;

        public BurnEffectInstance(
            EntityController target,
            AttackEffectData data,
            AttackSource attackSource,
            GameplayTag burnTag)
        {
            _target = target;
            _data = data;
            _attackSource = attackSource;

            _burnTag = burnTag;
            
            RemainingTime = data.duration;
            _tickTimer = 0f;
        }
        
        public override void OnApply()
        {
            _target.Tags.AddTag(_burnTag);
            _tickTimer = _data.tickInterval; // delay first tick
        }

        public override void OnRemove()
        {
            _target.Tags.RemoveTag(_burnTag);
        }
        
        public override void OnTick(float deltaTime)
        {
            if (_data.tickInterval <= 0f)
                return;
            
            _tickTimer -= deltaTime;
            if (_tickTimer > 0f)
                return;

            _tickTimer = _data.tickInterval;

            if (!_target.TryGetComponent<ICombatReceiver>(out var receiver))
                return;
            
            var payload = CombatPayloadFactory.CreateEffectDamage(
                damage: Mathf.RoundToInt(_data.value),  // TODO: In case we need modifiers to effect damage later, this is where we change.
                source: _attackSource
            );

            CombatExecutionPipeline.Execute(
                receiver,
                payload
            );
        }
    }
}
