using Game.Entity.Player.Subsystem;
using Game.Services.Save;
using UnityEngine;

namespace Game.Entity.Player.Progression.Util
{
    public static class CharacterPortraitResolver
    {
        // Ugly lookup to fetch the correct Portrait for each Character based on their Level/Evolution.
        // Used by UI classes.
        public static Sprite GetPortrait(CharacterDefinition definition, GameSaveManager saveManager)
        {
            if (definition == null)
                return null;

            Sprite fallback = definition.rosterScreenPortrait;

            if (definition.prefab == null || saveManager == null)
                return fallback;

            if (!definition.prefab.TryGetComponent(out CharacterLevelSubsystem levelSubsystem))
                return fallback;

            if (!definition.prefab.TryGetComponent(out CharacterEvolutionSubsystem evolutionSubsystem))
                return fallback;

            if (levelSubsystem.LevelUpData == null || evolutionSubsystem.EvolutionData == null || definition.baseStats == null)
                return fallback;

            string characterId = definition.baseStats.characterId;

            int xp = saveManager.GetCharacterXp(characterId);
            int level = levelSubsystem.LevelUpData.GetLevelForXp(xp);
            int stage = evolutionSubsystem.EvolutionData.GetStageForLevel(level);

            if (!evolutionSubsystem.EvolutionData.TryGetStage(stage, out CharacterEvolutionData.EvolutionStage stageData))
                return fallback;

            return stageData.portraitOverride != null ? stageData.portraitOverride : fallback;
        }
    }
}