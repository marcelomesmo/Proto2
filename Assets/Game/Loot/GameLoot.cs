using Core.Gameplay.Loot;
using Game.EventChannels;
using UnityEngine;

namespace Game.Loot
{
    // Game-specific loot implementation that extends BaseLoot.
    // Raises game-specific events with loot type information.
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(LootController))]
    public class GameLoot : BaseLoot
    {
        [Header("Game-Specific Broadcast")]
        [SerializeField] private LootCollectedEventChannelSO OnGameLootCollected;

        private GameLootSO _gameLootData;

        private void Awake()
        {
            base.Awake();
            
            // Cache the game-specific loot data
            _gameLootData = lootData as GameLootSO;
            
            if (_gameLootData == null)
            {
                Debug.LogError($"[GameLoot] LootData must be a GameLootSO! Current type: {lootData?.GetType().Name}", this);
            }
        }
        
        // Override Collect to raise game-specific event with loot type.
        // Calls base.Collect() to maintain core loot behavior (UI updates, etc.)
        public override void Collect()
        {
            base.Collect();
            
            if (_gameLootData)
            {
                // Raise game-specific event with type information
                OnGameLootCollected?.RaiseEvent(_gameLootData.lootType, _gameLootData.contribution);
            }
            else
            {
                Debug.LogWarning($"[GameLoot] GameLootSO is null, skipping game-specific event.", this);
            }
        }
    }
}
