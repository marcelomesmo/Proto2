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
            // Attack filter (matches actualAttack itself, or any attack it is a variant of)
            if (targetAttack != null &&
                !MatchesAttackChain(actualAttack))
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
        
        private bool MatchesAttackChain(AttackData actualAttack)
        {
            // This method is a bit too much, since we don't implement recursive attack variants, i.e. variant of variant of base.
            
            var current = actualAttack;
            int safety = 8; // guards against a misconfigured circular baseAttack chain

            while (current != null && safety-- > 0)
            {
                if (current == targetAttack)
                    return true;

                current = current.baseAttack;
            }

            return false;
        }
    }
}
