using System.Collections.Generic;
using Core.Upgrades.Runtime;

namespace Core.Services.Meta
{
    // Responsibility:
    // - Meta progression manager (upgrades unlock and tree manager).
    // Clear on every match.
    public sealed class UpgradeRuntimeManager
    {
        private readonly List<RuntimeUpgrade> _activeUpgrades = new();
        public IReadOnlyList<RuntimeUpgrade> ActiveUpgrades => _activeUpgrades;
        
        public void OnMatchStart()
        {
            _activeUpgrades.Clear();
        }

        public void OnMatchEnd()
        {
            _activeUpgrades.Clear();
        }
        
        // Called at match start.
        public void LoadUpgrades(IEnumerable<RuntimeUpgrade> upgrades)
        {
            _activeUpgrades.Clear();
            
            if (upgrades == null)
                return;
            
            _activeUpgrades.AddRange(upgrades);
        }
    }
}