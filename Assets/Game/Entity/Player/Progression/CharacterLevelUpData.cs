using System;
using UnityEngine;

namespace Game.Entity.Player.Progression
{
    [CreateAssetMenu(
        fileName = "CharacterLevelUpData",
        menuName = "Game/Character/Level Up Data")]
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
    }

    [Serializable]
    public struct LevelStatGain
    {
        public int attackPower;
        public int healingPower;
        public int defense;
    }
}