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
        public int version = 1;

        // Currency
        public int gold;

        // Unlocks
        public List<string> unlockedCharacters = new();
        public List<string> unlockedUpgrades = new();

        // Stats
        public int totalMatches;
        public int totalWins;

        public GameSave()
        {
            gold = 0;
        }
    }
}
