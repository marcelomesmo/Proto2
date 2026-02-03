using System;
using Core.Gameplay.Combat.Attack;
using UnityEngine;

namespace Game.Entity.Player.Progression
{
    [CreateAssetMenu(
        fileName = "CharacterEvolutionData",
        menuName = "Game/Character/Evolution Data")]
    public sealed class CharacterEvolutionData : ScriptableObject
    {
        [Serializable]
        public struct EvolutionStage
        {
            [Tooltip("Stage index (0-based)")]
            public int stage;

            [Tooltip("Level required to reach this stage")]
            public int requiredLevel;

            [Header("Presentation")]
            public Sprite portraitOverride;
            public Sprite spriteOverride;
            public RuntimeAnimatorController animatorOverride;

            [Header("Combat")]
            public AttackLoadoutDefinition attackLoadoutOverride;
        }

        [Tooltip("Stages must be ordered by stage index")]
        public EvolutionStage[] stages;

        public int MaxStage => stages.Length - 1;

        public bool TryGetStage(int stage, out EvolutionStage result)
        {
            if (stage < 0 || stage >= stages.Length)
            {
                result = default;
                return false;
            }

            result = stages[stage];
            return true;
        }

        public int GetStageForLevel(int level)
        {
            int result = 0;

            for (int i = 0; i < stages.Length; i++)
            {
                if (level >= stages[i].requiredLevel)
                    result = stages[i].stage;
            }

            return result;
        }
    }
}