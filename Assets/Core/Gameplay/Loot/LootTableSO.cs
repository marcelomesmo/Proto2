using System.Collections.Generic;
using UnityEngine;

namespace Core.Gameplay.Loot
{
    [CreateAssetMenu(fileName = "LootTableSO", menuName = "Loot/Loot Table")]
    public class LootTableSO : ScriptableObject
    {
        public List<LootEntry> entries;
    }

    [System.Serializable]
    public class LootEntry
    {
        [Header("Loot")]
        public LootSO loot;
        [Range(0f, 1f)]
        public float dropChance;
        
        [Header("Spawned Pickup Count")]
        [Min(0)]
        public int minAmount;
        [Min(0)]
        public int maxAmount;
        
        [Header("Contribution Per Pickup")]
        [Min(0)]
        public int minContribution = 1;
        [Min(0)]
        public int maxContribution = 1;

        public void Normalize()
        {
            if (maxAmount < minAmount)
                maxAmount = minAmount;

            if (maxContribution < minContribution)
                maxContribution = minContribution;
        }
    }
}