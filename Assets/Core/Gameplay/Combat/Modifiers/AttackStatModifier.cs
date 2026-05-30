using System;
using Core.Enum;
using Core.Gameplay.Combat.Attack;

namespace Core.Gameplay.Combat.Modifiers
{
    [Serializable]
    public struct AttackStatModifier
    {
        // Optional specific attack filter
        public AttackData targetAttack;
        
        // Target stat
        public AttackStatType statType;

        // General filters
        public ModifierScope scope;
        public ModifierType type;

        public float value;

        // Optional HitType filter
        public bool filterByHitType;
        public HitTypes hitTypes;

        public bool AppliesTo(
            AttackData actualAttack,
            AttackStatType actualStatType,
            ModifierScope actualScope,
            HitTypes actualHitTypes)
        {
            // Attack filter
            if (targetAttack != null &&
                targetAttack != actualAttack)
                return false;
            
            // StatType filter
            if (statType != actualStatType)
                return false;

            // Scope filter
            bool scopeMatch =
                scope == ModifierScope.All ||
                scope == actualScope;

            if (!scopeMatch)
                return false;

            // HitType filter
            if (!filterByHitType)
                return true;

            return (actualHitTypes & hitTypes) != 0;
        }
    }
}
