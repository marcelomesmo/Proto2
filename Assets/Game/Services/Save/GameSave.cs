using System;
using System.Collections.Generic;

namespace Game.Services.Save
{
    /*
     * Save data for the game.
     * Implementation in: GameSaveManager.
     */
    [Serializable]
    public sealed class GameSave
    {
        public int version = 3;

        // Currency
        public int gold;

        // Unlocks
        public List<string> unlockedCharacters = new();
        public List<string> unlockedLevels = new();
        public List<UpgradeProgress> upgrades = new();

        // Persistent party slots.
        public PartySlotSaveData[] partySlots =
        {
            new()
            {
                unlocked = true,
                characterId = string.Empty
            },
            new()
            {
                unlocked = false,
                characterId = string.Empty
            },
            new()
            {
                unlocked = false,
                characterId = string.Empty
            }
        };
        
        // Character progression
        public List<CharacterProgressSaveData> characterProgress = new();
        
        // Stats
        public int totalMatches;
        public int totalWins;
        
        // Progression position
        // Stored as zero-based indices.
        public int currentStageIndex;
        public int currentLevelIndex;

        public GameSave()
        {
            gold = 0;
        }
    }
    
    [Serializable]
    public sealed class UpgradeProgress
    {
        public string id;
        public int level;
    }
    
    [Serializable]
    public sealed class PartySlotSaveData
    {
        public bool unlocked;
        public string characterId = string.Empty;
    }
    
    [Serializable]
    public sealed class CharacterProgressSaveData
    {
        public string characterId;
        public int xp;
    }
}
