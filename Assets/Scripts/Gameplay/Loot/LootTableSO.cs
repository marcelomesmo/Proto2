using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Collectables
{
    [CreateAssetMenu(fileName = "LootTableSO", menuName = "Loot/Loot Table")]
    public class LootTableSO : ScriptableObject
    {
        public List<LootEntry> entries;
    }

    [System.Serializable]
    public class LootEntry
    {
        public ResourceSO resource;
        [Range(0f, 1f)]
        public float dropChance;
        public int minAmount;
        public int maxAmount;
    }
}