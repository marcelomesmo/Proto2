using System;
using Core.Services.Save;
using Core.Services.Save.Storage;
using UnityEngine;

namespace Game.Services.Save
{
    /*
     * Implementation and handling of the GameSave save data.
     * 
     * Responsibilities:
     * - Load/save GameSave.
     * - Persist currencies.
     * - Persist progression.
     * - Persist match statistics.
     *
     * Party rules are owned by PartyManager.
     */
    public sealed class GameSaveManager : MonoBehaviour, ISaveManager
    {
        [SerializeField] private SaveDescriptor descriptor;
        
        private SaveSerializer<GameSave> _save;

        // TODO: This needs to go to ISaveManager and become ISave.
        // Eventually expose a Core-level ISave instead of allowing
        // Game/Core progression systems to depend directly on GameSave.
        public GameSave Profile => _save.Data;

        // UI Events
        public event Action<int> OnGoldChanged;

        // --------------------------------------------------
        // Initialization
        // --------------------------------------------------

        public void Initialize()
        {
            Load();

            bool profileChanged = MigrateProfile();
            
            if (profileChanged)
                Save();
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
            /*Debug.Log($"[SaveManager] Loaded save. Gold: {Profile.gold}, Unlocked characters: {Profile.unlockedCharacters.Count}");
            foreach (var charId in Profile.unlockedCharacters)
            {
                Debug.Log($"[SaveManager] - Character ID: '{charId}'");
            }*/
        }

        public void Save() => _save.Save();
        public void Reset() => _save.Reset();
        
        // --------------------------------------------------
        // Save Migration
        // --------------------------------------------------

        //
        // The game is currently unreleased, so this is intentionally
        // minimal.
        //
        // Keep this entry point so actual release migrations can be
        // introduced later without restructuring initialization.
        //
        private bool MigrateProfile()
        {
            bool changed = false;

            if (Profile.version < 3)
            {
                Profile.version = 3;
                changed = true;
            }

            return changed;
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
            if (amount <= 0)
                return false;
            
            if (Profile.gold < amount)
                return false;

            Profile.gold -= amount;
            Save();
            
            OnGoldChanged?.Invoke(Profile.gold);
            return true;
        }
        
        // --------------------------------------------------
        // Characters
        // --------------------------------------------------
        
        // Query remains useful outside PartyManager.
        // Actual party assignment rules and first-character unlocking belong to PartyManager.
        public bool IsCharacterUnlocked(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return false;

            return Profile
                .unlockedCharacters
                .Contains(id);
        }
        
        public int GetCharacterXp(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
                return 0;

            foreach (CharacterProgressSaveData progress in Profile.characterProgress)
            {
                if (progress.characterId == characterId)
                    return progress.xp;
            }

            return 0;
        }

        public void SetCharacterXp(string characterId, int xp)
        {
            if (string.IsNullOrWhiteSpace(characterId))
                return;

            xp = Mathf.Max(0, xp);

            foreach (CharacterProgressSaveData progress in Profile.characterProgress)
            {
                if (progress.characterId != characterId)
                    continue;

                if (progress.xp == xp)
                    return;

                progress.xp = xp;
                Save();

                return;
            }

            // In case it's a new entry
            Profile.characterProgress.Add(
                new CharacterProgressSaveData
                {
                    characterId = characterId,
                    xp = xp
                });

            Save();
        }
        
        // ------------------------------------
        // Levels
        // ------------------------------------

        public bool IsLevelUnlocked(string id)
        {
            return Profile.unlockedLevels.Contains(id);
        }

        // ------------------------------------
        // Upgrades
        // ------------------------------------

        // Moved to UpgradeManager.cs
        
        // ------------------------------------
        // End-of-Match Save
        // ------------------------------------

        public void OnMatchEnd()
        {
            // Anything we need to save at the match end
        }
    }
}
