using System.Collections.Generic;
using Core.Gameplay.Combat.StatusEffect;
using UnityEngine;

namespace Core.Gameplay.Entity.Subsystem
{
    public class EntityStatusEffectSubsystem : BaseSubsystem
    {
        private readonly List<StatusEffectInstance> _activeEffects = new();

        protected override void OnUpdate()
        {
            float dt = Time.deltaTime;

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                effect.Tick(dt);

                if (!effect.IsExpired)
                    continue;
                
                effect.OnRemove();
                _activeEffects.RemoveAt(i);
            }
        }

        public void AddEffect(StatusEffectInstance effect)
        {
            effect.OnApply();
            _activeEffects.Add(effect);
        }
    }
}
