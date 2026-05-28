using UnityEngine;

namespace Core.Upgrades.Effects
{
    [CreateAssetMenu(
        fileName = "ExperienceBonusEffect",
        menuName = "Upgrades/Effects/Experience Bonus")]
    public sealed class ExperienceBonusEffect : UpgradeEffect
    {
        [Tooltip("Total XP multiplier per level. Index = level - 1. Example: [1.10, 1.15, 1.30].")]
        [SerializeField]
        private float[] multiplierPerLevel = { 1.10f, 1.15f, 1.30f };

        public override void Apply(UpgradeContext context, int level)
        {
            if (context == null || context.Modifiers == null)
                return;

            if (level <= 0)
                return;

            context.Modifiers.AddXpModifier(GetModifier(level));
        }

        public override void Remove(UpgradeContext context, int level)
        {
            if (context == null || context.Modifiers == null)
                return;

            if (level <= 0)
                return;

            context.Modifiers.RemoveXpModifier(GetModifier(level));
        }
        
        private ExperienceModifier GetModifier(int level) 
            => new ExperienceModifier { multiplier = GetMultiplierForLevel(level) };

        private float GetMultiplierForLevel(int level)
        {
            if (multiplierPerLevel == null || multiplierPerLevel.Length == 0)
                return 1f;

            int idx = Mathf.Clamp(level - 1, 0, multiplierPerLevel.Length - 1);
            return multiplierPerLevel[idx];
        }
        
        // TODO: Later we can add a CanApply() if we move the CharacterLevelSubsystem component to Core.
    }
}
