using System.Collections.Generic;
using UnityEngine;

namespace Core.Upgrades.Database
{
    [CreateAssetMenu(
        fileName = "UpgradeDatabase",
        menuName = "Upgrades/Upgrade Database")]
    public sealed class UpgradeDatabase : ScriptableObject, IUpgradeDatabase
    {
        [SerializeField]
        private List<UpgradeDefinition> upgrades;
        public IReadOnlyList<UpgradeDefinition> Upgrades => upgrades;

        private Dictionary<string, UpgradeDefinition> _map = new();

        private void OnEnable()
        {
            BuildIndex();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Keeps index valid in editor when you modify the list.
            BuildIndex();
        }
#endif

        public void BuildIndex()
        {
            _map.Clear();

            foreach (var upgrade in upgrades)
            {
                if (upgrade == null)
                    continue;

                if (string.IsNullOrWhiteSpace(upgrade.UpgradeId))
                {
                    Debug.LogWarning($"[UpgradeDatabase] Upgrade '{upgrade.name}' has empty upgradeId.", this);
                    continue;
                }

                _map[upgrade.UpgradeId] = upgrade;
            }
        }

        public bool TryGet(string id, out UpgradeDefinition definition)
        {
            definition = null;

            if (string.IsNullOrWhiteSpace(id))
                return false;

            // If this asset was never enabled (or list changed), rebuild lazily.
            if (_map.Count == 0 && upgrades != null && upgrades.Count > 0)
                BuildIndex();

            return _map.TryGetValue(id, out definition);
        }
    }
}
