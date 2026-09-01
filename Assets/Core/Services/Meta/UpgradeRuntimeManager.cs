using System;
using System.Collections.Generic;
using Core.Upgrades;
using Core.Upgrades.Runtime;
using UnityEngine;

namespace Core.Services.Meta
{
    // Responsibility:
    // - Meta progression manager (upgrades unlock and tree manager).
    // Clear on every match.
    public sealed class UpgradeRuntimeManager
    {
        private readonly List<RuntimeUpgrade> _activeUpgrades = new();
        public IReadOnlyList<RuntimeUpgrade> ActiveUpgrades => _activeUpgrades;
        
        public event Action<UpgradeDefinition, int, int> OnUpgradeLevelChanged;
        
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
        
        public void SetUpgradeLevel(UpgradeDefinition definition, int newLevel)
        {
            if (definition == null)
                return;

            newLevel = Mathf.Clamp(newLevel, 0, definition.maxLevel);

            for (int i = 0; i < _activeUpgrades.Count; i++)
            {
                RuntimeUpgrade runtime = _activeUpgrades[i];

                if (runtime.Definition != definition)
                    continue;

                int oldLevel = runtime.Level;

                if (oldLevel == newLevel)
                    return;

                if (newLevel <= 0)
                {
                    _activeUpgrades.RemoveAt(i);
                }
                else
                {
                    _activeUpgrades[i] = new RuntimeUpgrade(definition, newLevel);
                }

                OnUpgradeLevelChanged?.Invoke(definition, oldLevel, newLevel);

                return;
            }

            // Upgrade was not previously active.
            if (newLevel <= 0)
                return;

            _activeUpgrades.Add(new RuntimeUpgrade(definition, newLevel));

            OnUpgradeLevelChanged?.Invoke(definition, 0, newLevel);
        }
    }
}