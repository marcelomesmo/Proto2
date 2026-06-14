using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Combat.StatusEffect.Implementations
{
    public class StrikeEffectInstance : StatusEffectInstance
    {
        private readonly EntityController _target;
        private readonly AttackEffectData _data;
        private readonly AttackSource _attackSource;

        public StrikeEffectInstance(
            EntityController target,
            AttackEffectData data,
            AttackSource attackSource)
        {
            _target = target;
            _data = data;
            _attackSource = attackSource;
            
            RemainingTime = data.duration;
        }
        
        public override void OnApply()
        {
            DealDamageOnce();
            RemainingTime = 0f;          // expires on next subsystem update
        }

        public override void OnRemove()
        {
            // do nothing
        }
        
        public override void OnTick(float deltaTime)
        {
            // do nothing
        }
        
        private void DealDamageOnce()
        {
            if (!_target.TryGetComponent<ICombatReceiver>(out var damageable))
                return;

            var payload = CombatPayloadFactory.CreateEffectDamage(
                damage: Mathf.RoundToInt(_data.value),
                source: _attackSource,
                modifiers: null
            );

            CombatExecutionPipeline.Execute(
                damageable,
                payload
            );
        }
    }
}