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
        private readonly DamageSource _damageSource;

        public StrikeEffectInstance(
            EntityController target,
            AttackEffectData data,
            DamageSource damageSource)
        {
            _target = target;
            _data = data;
            _damageSource = damageSource;
            
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
            if (!_target.TryGetComponent<IDamageable>(out var damageable))
                return;

            var payload = DamagePayloadFactory.CreateEffectDamage(
                damage: Mathf.RoundToInt(_data.value),
                source: _damageSource,
                modifiers: null
            );

            damageable.TakeDamage(payload);
        }
    }
}