using System.Collections.Generic;
using Core.Upgrades.Effects;
using UnityEngine;

namespace Core.Upgrades
{
    public abstract class UpgradeDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string upgradeId;
        public string displayName;
        public string description;
        public Sprite icon;
        
        [Header("Progression")]
        public int maxLevel = 5;
        
        // Abstract methods that Game layer implements
        public abstract int GetCostForLevel(int level);
        public abstract string GetEffectDescriptionForLevel(int level);
        
        [Header("Effects")]
        [SerializeField]
        private List<UpgradeEffect> effects = new();
        public IReadOnlyList<UpgradeEffect> Effects => effects;
    }
}
