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
        public LootSO loot;
        [Range(0f, 1f)]
        public float dropChance;
        public int minAmount;
        public int maxAmount;
    }
}