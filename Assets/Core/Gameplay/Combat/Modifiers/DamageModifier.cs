using System;
using Core.Enum;
using Core.Gameplay.Combat.Attack;

namespace Core.Gameplay.Combat.Modifiers
{
    [Serializable]
    public struct DamageModifier
    {
        // Optional specific attack filter
        public AttackData targetAttack;
        
        // General filters
        public ModifierScope scope;
        public ModifierType type;
        
        public float value;
       
        // Optional HitType filter
        public bool filterByHitType;
        public HitTypes hitTypes;
        
        //public ElementTypes elementTypes;
        //public DamageTypes damageTypes;

        //public bool filterByElement;
        //public bool filterByDamageType;

        public bool AppliesTo(
            AttackData actualAttack, 
            ModifierScope actualScope, 
            HitTypes? actualHitTypes)
        {
            // Attack filter
            if (targetAttack != null &&
                targetAttack != actualAttack)
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
            
            /*
            // Element check
            if (filterByElement)
            {
                if (!actualElements.HasValue)
                    return false;

                if ((actualElements.Value & elementTypes) == 0)
                    return false;
            }
            
            // Damage type check
            if (filterByDamageType)
            {
                if (!actualDamageTypes.HasValue)
                    return false;

                if ((actualDamageTypes.Value & damageTypes) == 0)
                    return false;
            }
            */

            return (actualHitTypes & hitTypes) != 0; // Check the HitType flags.
        }
    }
}