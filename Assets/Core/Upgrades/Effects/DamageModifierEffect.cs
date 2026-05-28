using Core.Gameplay.Combat.Modifiers;
using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Core.Upgrades.Effects
{
    [CreateAssetMenu(
        fileName = "DamageModifierEffect",
        menuName = "Upgrades/Effects/Damage Modifier")]
    public sealed class DamageModifierEffect : UpgradeEffect
    {
        [Tooltip("Modifier per purchased level. Index = level - 1.")]
        [SerializeField]
        private DamageModifier[] modifierPerLevel;

        public override void Apply(UpgradeContext context, int level)
        {
            if (context == null || context.Modifiers == null || level <= 0)
                return;

            // Remove previous contribution from this effect
            //Remove(context, level - 1);

            // Add current level modifier
            context.Modifiers.AddDamageModifier(GetModifierForLevel(level));
        }

        public override void Remove(UpgradeContext context, int level)
        {
            if (context == null || context.Modifiers == null)
                return;

            if (level <= 0)
                return;

            context.Modifiers.RemoveDamageModifier(GetModifierForLevel(level));
        }

        private DamageModifier GetModifierForLevel(int level)
        {
            if (modifierPerLevel == null || modifierPerLevel.Length == 0)
                return default;

            int idx = Mathf.Clamp(level - 1, 0, modifierPerLevel.Length - 1);
            return modifierPerLevel[idx];
        }
        
        // Validate against having an EntityAttackSubsystem.
        public override bool CanApply(UpgradeContext context)
        {
            return context?.Entity?.GetComponent<EntityAttackSubsystem>() != null &&
                   context.Modifiers != null;
        }
    }
}
