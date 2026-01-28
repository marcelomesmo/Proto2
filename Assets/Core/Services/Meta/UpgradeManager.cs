using System.Collections.Generic;
using Core.Gameplay.Combat.Modifiers;
using Core.Upgrades;

namespace Core.Services.Meta
{
    // Runtime container for all upgrades active during a match.
    // Cleared and recreated per match.
    public class UpgradeManager
    {
        private readonly List<UpgradeDefinition> _activeUpgrades = new();
        
        public void OnMatchStart()
        {
            _activeUpgrades.Clear();
        }

        public void OnMatchEnd()
        {
            _activeUpgrades.Clear();
        }

        public void AddUpgrade(UpgradeDefinition upgrade)
        {
            if (upgrade != null)
                _activeUpgrades.Add(upgrade);
        }
        
        public void CollectDamageModifiers(
            ModifierScope scope,
            List<DamageModifier> output)
        {
            foreach (var upgrade in _activeUpgrades)
            {
                foreach (var mod in upgrade.damageModifiers)
                {
                    if (mod.scope == scope || mod.scope == ModifierScope.AllDamage)
                        output.Add(mod);
                }
            }
        }
        
        public IReadOnlyList<UpgradeDefinition> ActiveUpgrades => _activeUpgrades;
    }
}