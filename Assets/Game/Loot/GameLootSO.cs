using Core.Gameplay.Loot;
using Game.Enum;
using UnityEngine;

namespace Game.Loot
{
    // Game-specific loot data that extends the core LootSO.
    // Adds loot type identification for this game's specific resource system.
    [CreateAssetMenu(fileName = "GameLootSO", menuName = "Game/Loot/Game Loot Data")]
    public class GameLootSO : LootSO
    {
        [Header("Game-Specific Settings")]
        [Tooltip("What type of resource this loot represents (Gold, Wood, Stone, etc.)")]
        public LootType lootType = LootType.Gold;
        
        // Future: Add game-specific loot properties here
        // Example: public Rarity rarity;
        // Example: public bool isQuestItem;
        // Example: public AudioClip customCollectSound;
    }
}