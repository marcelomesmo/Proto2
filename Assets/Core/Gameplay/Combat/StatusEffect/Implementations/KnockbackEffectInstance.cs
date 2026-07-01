using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Core.Gameplay.Combat.StatusEffect.Implementations
{
    public class KnockbackEffectInstance : StatusEffectInstance
    {
        private readonly EntityController _target;
        private readonly AttackSource _source;

        public KnockbackEffectInstance(
            EntityController target,
            StatusEffectData data,
            AttackSource source)
        {
            _target = target;
            _source = source;
            SourceData = data;
            RemainingTime = data.duration;
        }
        
        public override void OnApply()
        {
            _target.Tags.AddTag(SourceData.tagToApply);
            
            var movement = _target.GetComponent<EntityMovement>();
            if (movement == null)
            {
                Debug.Log("[KnockbackEffectInstance] Trying to knockback an unmovable entity " + _target.name + " No movement component attached.");
                return;
            }
            
            // Direction away from the attacker
            Vector2 awayFromSource =
                (Vector2)_target.transform.position - _source.sourcePosition;

            if (awayFromSource.sqrMagnitude < 0.0001f)
                awayFromSource = Vector2.right;
            
            Vector2 direction = awayFromSource.normalized;
            
            float duration = Mathf.Max(SourceData.duration, Time.fixedDeltaTime);
            
            // Treat SourceData.value as knockback distance.
            float distance = SourceData.value;
            float speed = distance / duration;
            
            movement.ApplyKnockback(direction * speed, duration);
        }

        public override void OnRemove()
        {
            _target.Tags.RemoveTag(SourceData.tagToApply);
        }
    }
}
