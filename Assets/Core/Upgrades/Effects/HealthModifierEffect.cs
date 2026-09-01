using Core.Gameplay.Combat.Modifiers;
using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Core.Upgrades.Effects
{
    [CreateAssetMenu(
        fileName = "HealthModifierEffect",
        menuName = "Upgrades/Effects/Health Modifier")]
    public sealed class HealthModifierEffect : UpgradeEffect
    {
        [SerializeField]
        private HealthModifier[] modifierPerLevel;

        public override void Apply(UpgradeContext context, int level)
        {
            if (context == null || context.Modifiers == null || level <= 0)
                return;

            context.Modifiers.AddHealthModifier(
                GetModifierForLevel(level));
            
            RefreshHealth(context);
        }

        public override void Remove(UpgradeContext context, int level)
        {
            if (context == null || context.Modifiers == null || level <= 0)
                return;

            context.Modifiers.RemoveHealthModifier(
                GetModifierForLevel(level));
            
            RefreshHealth(context);
        }

        private void RefreshHealth(
            UpgradeContext context)
        {
            if (!context.Entity.TryGetComponent(out EntityHealth health))
                return;

            health.RefreshMaxHealthAfterModifierChange();
        }
        
        private HealthModifier GetModifierForLevel(int level)
        {
            if (modifierPerLevel == null || modifierPerLevel.Length == 0)
                return default;

            int idx = Mathf.Clamp(level - 1, 0, modifierPerLevel.Length - 1);
            return modifierPerLevel[idx];
        }
        
        // Validate against having an EntityHealthSubsystem.
        public override bool CanApply(UpgradeContext context)
        {
            return context?.Entity?.GetComponent<EntityHealth>() != null &&
                   context.Modifiers != null;
        }
    }
}