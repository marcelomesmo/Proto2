using System.Collections.Generic;
using Core.Upgrades;

namespace Core.Services.Meta
{
    // Runtime registry of unlocked upgrades for this match
    public class UpgradeManager
    {
        private readonly List<UpgradeDefinition> _activeUpgrades = new();
        public IReadOnlyList<UpgradeDefinition> ActiveUpgrades => _activeUpgrades;
        
        public void OnMatchStart()
        {
            // Nothing else here anymore
        }

        public void OnMatchEnd()
        {
            _activeUpgrades.Clear();
        }
        
        // Called at match start
        public void LoadUpgrades(IEnumerable<UpgradeDefinition> upgrades)
        {
            _activeUpgrades.Clear();
            _activeUpgrades.AddRange(upgrades);
        }
    }
}