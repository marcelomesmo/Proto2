using System.Collections.Generic;
using Core.Gameplay.Combat;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.StatusEffect;
using UnityEngine;

namespace Core.Gameplay.Entity.Subsystem
{
    public class EntityStatusEffectSubsystem : BaseSubsystem
    {
        private readonly List<StatusEffectInstance> _activeEffects = new();
        
        // TODO: Later, if we want to add timers to status icons, we subscribe to these events in EntityPresentationSubsystem.
        // PS: we are actually doing this now in the EntityStatusIconController that is being called by the EntityPresentationSubsystem, check that to avoid duplicate.
        // public event Action<StatusEffectInstance> OnStatusAdded;
        // public event Action<StatusEffectInstance> OnStatusRemoved;
        // public event Action<StatusEffectInstance> OnStatusUpdated;

        protected override void OnInitialize()
        {
            _activeEffects.Clear();
        }
        
        protected override void OnDeinitialize()
        {
            // Remove in reverse and call OnRemove for cleanup (tags, multipliers, etc.)
            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                _activeEffects[i].OnRemove();
            }
            _activeEffects.Clear();
        }
        
        protected override void OnUpdate()
        {
            float dt = Time.deltaTime;

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                effect.Tick(dt);
                // OnStatusUpdated?.Invoke(effect);

                if (!effect.IsExpired)
                    continue;
                
                effect.OnRemove();
                // OnStatusRemoved?.Invoke(effect);
                _activeEffects.RemoveAt(i);
            }
        }

        public void AddEffect(StatusEffectInstance effect)
        {
            // Check for an existing instance of the same type from the same source data.
            // This avoids stacking the same application on every tick of an attack.
            // Correct behavior now is: apply effect on first tick -> refresh if applied again.
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                if (_activeEffects[i].SourceData == effect.SourceData)
                {
                    // Refresh duration — don't apply a second instance.
                    _activeEffects[i].RefreshDuration();
                    return;
                }
            }
            
            effect.OnApply();
            
            // If it expires immediately, remove it right now.
            if (effect.IsExpired)
            {
                effect.OnRemove(); // usually no-op for effects like Strike, but consistent lifecycle
                return;
            }
            
            _activeEffects.Add(effect);
            //OnStatusAdded?.Invoke(effect);
        }

        public void ApplyEffectsFromPayload(CombatPayload payload)
        {
            foreach (var effect in payload.effects)
            {
                var instance =
                    StatusEffectFactory.Create(
                        effect,
                        Controller,
                        payload.source);

                AddEffect(instance);
            }
        }
        
        public void RemoveEffect(StatusEffectData sourceData)
        {
            if (sourceData == null)
                return;

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];

                if (effect.SourceData != sourceData)
                    continue;

                effect.OnRemove();
                _activeEffects.RemoveAt(i);
            }
        }
    }
}
