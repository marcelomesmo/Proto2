using System;
using System.Collections.Generic;
using Core.Services.Meta;
using Game.Enum;

namespace Game.Services.Meta
{
    // Game-specific match statistics tracking.
    // Tracks enemies killed, waves cleared, and loot collected during a match.
    public sealed class GameMatchStats : MatchStats
    {
        // Combat Stats
        public int EnemiesKilled { get; private set; }
        public int WavesCleared { get; private set; }
        
        // Loot tracking - using dictionary for extensibility with multiple loot types
        private Dictionary<LootType, int> _lootCollected = new Dictionary<LootType, int>();
        // Read-only access to all loot collected during the current match.
        // Key: LootType, Value: Amount collected
        public IReadOnlyDictionary<LootType, int> LootCollected => _lootCollected;

        // Events for UI/external systems
        public event Action<int> OnEnemiesKilledChanged;
        public event Action<int> OnWaveClearedChanged;  // todo: add to analytics screen? end of level ui?
        public event Action<float> OnMatchTimeChanged;
        public event Action<LootType, int> OnLootCollectedChanged;
        
        public override void OnMatchStart()
        {
            EnemiesKilled = 0;
            WavesCleared = 0;
            _lootCollected.Clear();
        }
        
        public override void OnMatchTimeUpdated(float time)
        {
            OnMatchTimeChanged?.Invoke(time);
        }
        
        public void RegisterEnemyKilled()
        {
            EnemiesKilled++;
            OnEnemiesKilledChanged?.Invoke(EnemiesKilled);
        }
        public void RegisterWaveCleared()
        {
            WavesCleared++;
            OnWaveClearedChanged?.Invoke(WavesCleared);
        }
        
        public void RegisterLootCollected(LootType lootType, int amount)
        {
            if (amount <= 0)
            {
                UnityEngine.Debug.LogWarning($"[GameMatchStats] Attempting to register {amount} of {lootType}. Amount should be positive.");
                return;
            }
            
            // Initialize the dictionary entry if it doesn't exist. In case of the first time resource collected.
            if (!_lootCollected.ContainsKey(lootType))
                _lootCollected[lootType] = 0;
            
            // Add to the total
            _lootCollected[lootType] += amount;
            
            // Raise event with the NEW TOTAL for this loot type
            OnLootCollectedChanged?.Invoke(lootType, _lootCollected[lootType]);
        }
        
        //
        // Utility Methods
        //
        
        #region Utility Methods
        
        public int GetGoldCollected() => _lootCollected.GetValueOrDefault(LootType.Gold, 0);
        public int GetLootCollected(LootType lootType) => _lootCollected.GetValueOrDefault(lootType, 0);
        
        #endregion
    }
}
