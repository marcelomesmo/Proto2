using System.Collections.Generic;
using Core.Upgrades.Effects;
using UnityEngine;

namespace Core.Upgrades
{
    [CreateAssetMenu(
        fileName = "UpgradeDefinition",
        menuName = "Upgrades/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string upgradeId;
        public string displayName;
        public string description;
        public Sprite icon;
        
        [Header("Progression")]
        public int maxLevel = 5;
        
        [Tooltip("Cost per level (index = level - 1)")]
        [SerializeField]
        private int[] levelCosts;

        [Tooltip("Description per level (index = level - 1)")]
        [SerializeField]
        private string[] levelDescriptions;
        
        [Header("Effects")]
        [SerializeField]
        private List<UpgradeEffect> effects = new();
        public IReadOnlyList<UpgradeEffect> Effects => effects;
        
        // --------------------

        public int GetCostForLevel(int level)
        {
            if (level <= 0 || level > levelCosts.Length)
                return int.MaxValue;

            return levelCosts[level - 1];
        }

        public string GetEffectDescriptionForLevel(int level)
        {
            if (level <= 0 || level > levelDescriptions.Length)
                return string.Empty;

            return levelDescriptions[level - 1];
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (levelCosts == null ||
                levelCosts.Length != maxLevel)
            {
                Debug.LogWarning(
                    $"[UpgradeDefinition] '{name}' levelCosts length should be {maxLevel}",
                    this);
            }

            if (levelDescriptions == null ||
                levelDescriptions.Length != maxLevel)
            {
                Debug.LogWarning(
                    $"[UpgradeDefinition] '{name}' levelDescriptions length should be {maxLevel}",
                    this);
            }
        }
#endif
    }
}
