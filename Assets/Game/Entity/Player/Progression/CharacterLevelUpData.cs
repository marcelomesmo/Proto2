using System;
using UnityEngine;

namespace Game.Entity.Player.Progression
{
    [CreateAssetMenu(fileName = "CharacterLevelUpData", menuName = "Game/Character/Level Up Data")]
    public sealed class CharacterLevelUpData : ScriptableObject
    {
        [Header("Leveling")]
        [Tooltip("XP required to reach each level (index = target level)")]
        public int[] xpThresholds;

        [Header("Per-Level Stat Gains")]
        public LevelStatGain[] statGains;

        public int MaxLevel => xpThresholds.Length - 1;

        public int GetXpForLevel(int level)
        {
            if (level < 0 || level >= xpThresholds.Length)
                return int.MaxValue;

            return xpThresholds[level];
        }

        public LevelStatGain GetStatGainForLevel(int level)
        {
            if (level < 0 || level >= statGains.Length)
                return default;

            return statGains[level];
        }
        
        private void OnValidate()
        {
            if (statGains.Length != xpThresholds.Length)
                Debug.LogWarning(
                    $"[CharacterProgressionData] statGains and xpThresholds length mismatch",
                    this);
        }
        
        // ------------------
        // External Util
        // ------------------
        
        public int GetLevelForXp(int xp)
        {
            if (xpThresholds == null || xpThresholds.Length == 0)
                return 0;

            int level = 0;

            for (int i = 1; i < xpThresholds.Length; i++)
            {
                if (xp < xpThresholds[i])
                    break;

                level = i;
            }

            return level;
        }
        
        public float GetLevelProgress(int xp)
        {
            int level = GetLevelForXp(xp);

            if (level >= MaxLevel)
                return 1f;

            int currentLevelXp = GetXpForLevel(level);
            int nextLevelXp = GetXpForLevel(level + 1);
            int requiredXp = nextLevelXp - currentLevelXp;

            if (requiredXp <= 0)
                return 1f;

            int progressXp = xp - currentLevelXp;

            return Mathf.Clamp01(progressXp / (float)requiredXp);
        }
    }

    [Serializable]
    public struct LevelStatGain
    {
        public int attackPower;
        public int healingPower;
        public int defense;
    }
}