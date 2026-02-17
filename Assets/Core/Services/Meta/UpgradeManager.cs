using System.Collections.Generic;
using Core.Gameplay.Combat.Modifiers;
using Core.Upgrades;
using Core.Upgrades.Effects;

namespace Core.Services.Meta
{
    // Runtime container for all upgrades active during a match.
    // Cleared and recreated per match.
    public class UpgradeManager
    {
        private readonly List<UpgradeDefinition> _activeUpgrades = new();
        private readonly List<UpgradeContext> _contexts = new();
        
        public IReadOnlyList<UpgradeDefinition> ActiveUpgrades => _activeUpgrades;
        
        public void OnMatchStart()
        {
            _activeUpgrades.Clear();
            _contexts.Clear();
        }

        public void OnMatchEnd()
        {
            DeactivateAll();
            _activeUpgrades.Clear();
            _contexts.Clear();
        }

        public void AddUpgrade(UpgradeDefinition upgrade)
        {
            if (!upgrade)
                return;
            
            _activeUpgrades.Add(upgrade);

            var context = new UpgradeContext(this);
            _contexts.Add(context);
            
            ActivateUpgrade(upgrade, context);
        }
        
        private void ActivateUpgrade(
            UpgradeDefinition upgrade,
            UpgradeContext context)
        {
            foreach (var effect in upgrade.Effects)
            {
                effect.Apply(context);
            }
        }
        
        private void DeactivateAll()
        {
            for (int i = 0; i < _activeUpgrades.Count; i++)
            {
                var upgrade = _activeUpgrades[i];
                var context = _contexts[i];

                foreach (var effect in upgrade.Effects)
                {
                    effect.Remove(context);
                }
            }
        }
        
        // Compatibility Layer (Phase 1)
        public void CollectDamageModifiers(
            ModifierScope scope,
            List<DamageModifier> output)
        {
            foreach (var upgrade in _activeUpgrades)
            {
                foreach (var effect in upgrade.Effects)
                {
                    if (effect is not DamageModifierEffect dmgEffect)
                        continue;
                    
                    foreach (var mod in dmgEffect.modifiers)
                    {
                        if (mod.scope == scope ||
                            mod.scope == ModifierScope.AllDamage)
                        {
                            output.Add(mod);
                        }
                    }
                }
            }
        }
        
        public float GetExperienceMultiplier()
        {
            float multiplier = 1f;

            foreach (var context in _contexts)
            {
                foreach (var mod in context.ExperienceModifiers)
                {
                    multiplier *= mod.multiplier;
                }
            }

            return multiplier;
        }
    }
}