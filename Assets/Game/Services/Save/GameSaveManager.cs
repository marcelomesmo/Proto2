using System;
using Core.Services;
using Core.Services.Save;
using Core.Services.Save.Storage;
using Game.Services.Meta;
using UnityEngine;

namespace Game.Services.Save
{
    /*
     * Implementation and handling of the GameSave save data.
     */
    public sealed class GameSaveManager : MonoBehaviour, ISaveManager
    {
        [SerializeField] private SaveDescriptor descriptor;
        
        // TODO: deprecate this later when we do FTUE to unlock initial character.
        [Header("TEMP - Initial setup")]
        [SerializeField] private InitialProgressionData defaults;
        
        private SaveSerializer<GameSave> _save;

        public GameSave Profile => _save.Data;

        // UI Events
        public event Action<int> OnGoldChanged;
        public event Action<string> OnCharacterUnlocked;
        public event Action<string> OnUpgradeUnlocked;

        // ------------------------------------

        public void Initialize()
        {
            Load();
            
            // todo; deprecate
            if (Profile.unlockedCharacters.Count == 0 &&
                Profile.totalMatches == 0)  // is first run
                ApplyDefaults();
        }

        public void Load()
        {
            var storage = new LocalStorageProvider();

            /*
             Later:
                IStorageProvider storage =
                    Application.platform == RuntimePlatform.WindowsPlayer
                        ? new SteamCloudProvider()
                        : new LocalStorageProvider();
             */

            _save = new SaveSerializer<GameSave>(
                descriptor,
                storage);

            _save.Load();
            
            // DEBUG: Log what was loaded
            Debug.Log($"[SaveManager] Loaded save. Gold: {Profile.gold}, Unlocked characters: {Profile.unlockedCharacters.Count}");
            foreach (var charId in Profile.unlockedCharacters)
            {
                Debug.Log($"[SaveManager] - Character ID: '{charId}'");
            }
        }

        public void Save() => _save.Save();
        public void Reset() => _save.Reset();
        
        // todo; deprecate
        private void ApplyDefaults()
        {
            if (!defaults)
            {
                Debug.LogWarning("[Save] No default progression data.");
                return;
            }

            Profile.gold = defaults.startingGold;

            foreach (var id in defaults.startingUnlockedCharacters)
            {
                Profile.unlockedCharacters.Add(id);
            }

            Save();

            Debug.Log("[Save] Applied default progression.");
        }

        // ------------------------------------
        // Currency
        // ------------------------------------

        public int Gold => Profile.gold;

        public void AddGold(int amount)
        {
            if (amount <= 0)
                return;

            Profile.gold += amount;
            Save();
            OnGoldChanged?.Invoke(Profile.gold);
        }

        public bool SpendGold(int amount)
        {
            if (Profile.gold < amount)
                return false;

            Profile.gold -= amount;
            Save();
            OnGoldChanged?.Invoke(Profile.gold);
            return true;
        }

        // ------------------------------------
        // Characters
        // ------------------------------------

        public bool IsCharacterUnlocked(string id)
        {
            return Profile.unlockedCharacters.Contains(id);
        }

        public void UnlockCharacter(string id)
        {
            if (Profile.unlockedCharacters.Contains(id)) 
                return;
            
            Profile.unlockedCharacters.Add(id);
            OnCharacterUnlocked?.Invoke(id);
            Save();
        }

        // ------------------------------------
        // Upgrades
        // ------------------------------------

        public bool HasUpgrade(string id)
        {
            return Profile.unlockedUpgrades.Contains(id);
        }

        public void UnlockUpgrade(string id)
        {
            if (Profile.unlockedUpgrades.Contains(id)) 
                return;
            
            Profile.unlockedUpgrades.Add(id);
            OnUpgradeUnlocked?.Invoke(id);
            Save();
        }

        // ------------------------------------
        // Match Stats
        // ------------------------------------

        public void RegisterMatch(bool win)
        {
            Profile.totalMatches++;

            if (win)
                Profile.totalWins++;

            Save();
        }
        
        // ------------------------------------
        // End-of-Level Save
        // ------------------------------------

        public void OnMatchEnd()
        {
            RegisterMatch(false);   // todo: Implement victory/defeat logic (pass from gamecontroller or use global) if we want to distinguish for registering.
            
            // Save gold collected during the match
            var gameController = ServiceLocator.Get<GameController>();
            if (gameController?.MatchStats is GameMatchStats matchStats)
            {
                int goldCollected = matchStats.GetGoldCollected();
                if (goldCollected > 0)
                {
                    // This is adding and saving Gold to local save: Profile.
                    AddGold(goldCollected);
                    Debug.Log($"[SaveManager] Match ended. Gold collected: {goldCollected}. Total gold: {Profile.gold}");
                }
                
                /*
                 
                In the future, if we want to get any specific loot type:
                
                // Get all loot collected
                IReadOnlyDictionary<LootType, int> allLoot = matchStats.LootCollected;
                
                // Get specific loot type using the generic method
                int woodCollected = matchStats.GetLootCollected(LootType.Wood);
                int stoneCollected = matchStats.GetLootCollected(LootType.Stone);
                
                // Or get using the convenience method
                int gold = matchStats.GetGoldCollected();
                
                // Iterate over all collected loot
                foreach (var kvp in matchStats.LootCollected)
                {
                    Debug.Log($"Collected {kvp.Value} of {kvp.Key}");
                }
                
                And also, for full saving:
                
                // Save all loot types collected during the match
                foreach (var kvp in matchStats.LootCollected)
                {
                    if (kvp.Value <= 0) continue; // Skip if nothing collected
                    
                    switch (kvp.Key)
                    {
                        case LootType.Gold:
                            AddGold(kvp.Value);
                            break;
                            
                        case LootType.Wood:
                            AddWood(kvp.Value); // You'll need to add this method
                            break;
                            
                       default:
                            Debug.LogWarning($"[SaveManager] Unhandled loot type: {kvp.Key}");
                            break;     
                    }
                }
                 */
            }
        }
    }
}
