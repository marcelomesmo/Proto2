using System;
using Core.Gameplay.Loot;
using Game.Enum;
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
        
        public event Action<GameLoot, LootCollectionSource> GameLootCollected;
        
        private GameLootSO _gameLootData;
        
        public LootType LootType => _gameLootData ? _gameLootData.lootType : LootType.Unknown;
        public int LootValue => RuntimeContribution;
        public Sprite LootIcon => _gameLootData ? _gameLootData.icon : null;
        
        public override void Initialize(LootSO data, int contribution)
        {
            base.Initialize(data, contribution);
            CacheGameLootData();
        }

        private void CacheGameLootData()
        {
            _gameLootData = LootData as GameLootSO;

            if (_gameLootData == null)
            {
                Debug.LogError(
                    $"[GameLoot] LootData must be a GameLootSO. Current type: {LootData?.GetType().Name ?? "null"}",
                    this
                );
            }
        }

        protected override void OnCollected(LootCollectionSource source)
        {
            base.OnCollected(source);

            if (_gameLootData)
            {
                GameLootCollected?.Invoke(this, source);
                
                OnGameLootCollected?.RaiseEvent(
                    _gameLootData.lootType,
                    RuntimeContribution
                );
            }
            else
            {
                Debug.LogWarning(
                    "[GameLoot] GameLootSO is null, skipping game-specific event.",
                    this
                );
            }
        }
    }
}
