using Core.Enum;
using Core.Gameplay.Combat.Modifiers;
using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Core.Upgrades.Effects
{
    [CreateAssetMenu(
        fileName = "ResistanceStatModifierEffect",
        menuName = "Upgrades/Effects/Resistance Modifier")]
    public sealed class ResistanceStatModifierEffect : UpgradeEffect
    {
        [SerializeField]
        private AttackStatModifier[] modifierPerLevel;

        public override void Apply(UpgradeContext context, int level)
        {
            if (context == null || context.Modifiers == null || level <= 0)
                return;

            context.Modifiers.AddAttackStatModifier(GetModifierForLevel(level));
        }

        public override void Remove(UpgradeContext context, int level)
        {
            if (context == null || context.Modifiers == null || level <= 0)
                return;

            context.Modifiers.RemoveAttackStatModifier(GetModifierForLevel(level));
        }

        private AttackStatModifier GetModifierForLevel(int level)
        {
            if (modifierPerLevel == null || modifierPerLevel.Length == 0)
                return default;

            int idx = Mathf.Clamp(level - 1, 0, modifierPerLevel.Length - 1);
            return modifierPerLevel[idx];
        }

        // Gated on EntityHealth since defense upgrades target the Castle, not Characters.
        public override bool CanApply(UpgradeContext context)
        {
            return context?.Entity?.GetComponent<EntityHealth>() != null &&
                   context.Modifiers != null;
        }
                
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (modifierPerLevel == null)
                return;

            for (int i = 0; i < modifierPerLevel.Length; i++)
                modifierPerLevel[i].statType = AttackStatType.Resistance;
        }
#endif
    }
}
