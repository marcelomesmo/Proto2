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
        public int version = 2;

        // Currency
        public int gold;

        // Unlocks
        public List<string> unlockedCharacters = new();
        public List<UpgradeProgress> upgrades = new();

        // Stats
        public int totalMatches;
        public int totalWins;

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
}
