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

            Vector2 direction = GetKnockbackDirection();
            
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

        /*public Vector2 GetKnockbackDirection()
        {
            // Direction away from the attacker (doesn't work for multidimensional entities like a firewall)
            //Vector2 awayFromSource =
            //    (Vector2)_target.transform.position - _source.sourcePosition;
            //if (awayFromSource.sqrMagnitude < 0.0001f)
            //    awayFromSource = Vector2.right;
            //Vector2 direction = awayFromSource.normalized;
            
            float dx =
                _target.transform.position.x - _source.sourcePosition.x;

            float xDirection;

            if (Mathf.Abs(dx) > 0.01f)
            {
                xDirection = Mathf.Sign(dx);
            }
            else
            {
                // Fallback for cases where the target is almost exactly aligned
                // with the source center.
                //
                // Ideally this should come from the attack direction or caster facing.
                xDirection = _source.sourceEntity != null &&
                             _target.transform.position.x >= _source.sourceEntity.transform.position.x
                    ? 1f
                    : -1f;
            }

            Vector2 direction = new Vector2(xDirection, 0f);

            return direction;
            
            OR 
            
            if we want the entity to always be pushed backwards to where its facing:
                Vector2 facing = _target.FacingVector;

                if (facing.sqrMagnitude < 0.0001f)
                    return Vector2.right;

                return -facing.normalized;
        }*/
        
        // TODO: Later we can create a formula if we are working in a multidimensional where
        public Vector2 GetKnockbackDirection()
        {
            return Vector2.right;
        }
    }
}
