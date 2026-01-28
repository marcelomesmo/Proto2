using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.Modifiers;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Tags;
using Core.Interfaces;
using Core.Services;
using Core.Services.Meta;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Gameplay.Combat.StatusEffect.Implementations
{
    public class BurnEffectInstance : StatusEffectInstance
    {
        private readonly EntityController _target;
        private readonly AttackEffectData _data;
        private readonly DamageSource _source;
        private float _tickTimer;
        
        private readonly GameplayTag _burnTag;

        public BurnEffectInstance(
            EntityController target,
            AttackEffectData data,
            DamageSource source,
            GameplayTag burnTag)
        {
            _target = target;
            _data = data;
            _source = source;

            _burnTag = burnTag;
            
            RemainingTime = data.duration;
            _tickTimer = 0f;
        }
        
        public override void OnApply()
        {
            _target.Tags.AddTag(_burnTag);
        }

        public override void OnRemove()
        {
            _target.Tags.RemoveTag(_burnTag);
        }
        
        public override void OnTick(float deltaTime)
        {
            _tickTimer -= deltaTime;

            if (_tickTimer > 0f)
                return;

            _tickTimer = _data.tickInterval;

            if (!_target.TryGetComponent<IDamageable>(out var damageable))
                return;
            
            var modifiers = ListPool<DamageModifier>.Get();
            
            ServiceLocator
                .Get<GameController>()?
                .UpgradeManager
                .CollectDamageModifiers(
                    ModifierScope.Effect,
                    modifiers);
            
            var payload = DamagePayload.CreateEffectDamage(
                damage: Mathf.RoundToInt(_data.value),
                source: _source,
                hitPoint: _target.transform.position,
                modifiers: modifiers
            );

            damageable.TakeDamage(payload);
            
            ListPool<DamageModifier>.Release(modifiers);
        }
    }
}
