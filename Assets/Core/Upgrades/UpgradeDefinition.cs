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
        
        [Header("Unlock Requirements")]
        [SerializeField] private UpgradeUnlockRule upgradeUnlockRule;
        public UpgradeUnlockRule UnlockRule => upgradeUnlockRule;
        
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
            if (level < 0 || level > levelDescriptions.Length)
                return string.Empty;
            
            return levelDescriptions[level - 1];
        }
        
        public string GetEffectDescriptionForNextLevel(int currentLevel)
        {
            if(currentLevel == levelDescriptions.Length )
                return GetEffectDescriptionForLevel(currentLevel);  // stops when at last level,
                                                                    // obs: considers levelDescriptions size is correct.
            
            return GetEffectDescriptionForLevel(currentLevel+1);
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            //
            //  Level Cost check
            //
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
            
            //
            //  Effects check
            //
            if (Effects == null) return;

            var set = new HashSet<UpgradeEffect>();
            for (int i = 0; i < Effects.Count; i++)
            {
                if (Effects[i] != null && !set.Add(Effects[i]))
                {
                    Debug.LogWarning($"Duplicate effect in {name}: {Effects[i].name}", this);
                }
            }
            
            //
            //  Requirements check
            //
            if (upgradeUnlockRule?.Requirements == null)
                return;

            foreach (var requirement in upgradeUnlockRule.Requirements)
            {
                if (requirement == null)
                    continue;

                if (requirement.Upgrade == null)
                {
                    Debug.LogWarning(
                        $"[UpgradeDefinition] '{name}' contains a null requirement.",
                        this);

                    continue;
                }

                if (requirement.Upgrade == this)
                {
                    Debug.LogWarning(
                        $"[UpgradeDefinition] '{name}' cannot require itself.",
                        this);
                }
            }
        }
#endif
    }
}
