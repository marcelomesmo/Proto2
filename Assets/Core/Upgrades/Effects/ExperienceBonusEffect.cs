using UnityEngine;

namespace Core.Upgrades.Effects
{
    [CreateAssetMenu(
        fileName = "ExperienceBonusEffect",
        menuName = "Upgrades/Effects/Experience Bonus")]
    public sealed class ExperienceBonusEffect : UpgradeEffect
    {
        [Tooltip("Example: 0.1 = +10% XP")]
        [Range(0f, 5f)]
        public float bonusMultiplier = 0.1f;

        private ExperienceModifier _modifier;

        public override void Apply(UpgradeContext context)
        {
            _modifier = new ExperienceModifier
            {
                multiplier = 1f + bonusMultiplier
            };

            context.Modifiers.AddXpModifier(_modifier);
        }

        public override void Remove(UpgradeContext context)
        {
            context.Modifiers.RemoveXpModifier(_modifier);
        }
    }
}
