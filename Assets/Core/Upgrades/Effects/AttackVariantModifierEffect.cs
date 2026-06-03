using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Core.Upgrades.Effects
{
    [CreateAssetMenu(
        fileName = "AttackStatModifierEffect", 
        menuName = "Upgrades/Effects/Attack Variant Modifier")]
    public sealed class AttackVariantModifierEffect : UpgradeEffect
    {
        [SerializeField]
        private AttackVariantModifier[] modifierPerLevel;
    
        public override void Apply(UpgradeContext context, int level)
        {
            if (context == null || context.Modifiers == null || level <= 0)
                return;

            context.Modifiers.AddAttackVariantModifier(GetModifierForLevel(level));
        }
    
        public override void Remove(UpgradeContext context, int level)
        {
            if (context == null || context.Modifiers == null)
                return;

            if (level <= 0)
                return;

            context.Modifiers.RemoveAttackVariantModifier(GetModifierForLevel(level));
        }
    
        private AttackVariantModifier GetModifierForLevel(int level)
        {
            if (modifierPerLevel == null || modifierPerLevel.Length == 0)
                return default;

            int idx = Mathf.Clamp(level - 1, 0, modifierPerLevel.Length - 1);
            return modifierPerLevel[idx];
        }
        
        // Validate against having an EntityAttackSubsystem.
        // Implicit validate EntityModifierSubsystem as well (mandatory in EntityAttackSubsystem).
        public override bool CanApply(UpgradeContext context)
        {
            return context?.Entity?.GetComponent<EntityAttackSubsystem>() != null &&
                   context.Modifiers != null;
        }
    }
}
