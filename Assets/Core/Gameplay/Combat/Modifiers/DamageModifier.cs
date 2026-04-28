using System;
using Core.Enum;

namespace Core.Gameplay.Combat.Modifiers
{
    public enum ModifierType
    {
        Additive,           // +10 damage
        Multiplicative      // x1.2 damage
    }

    public enum ModifierScope
    {
        AllDamage,
        Melee,
        Projectile,
        Area,
        Chain,
        Effect
    }
    
    [Serializable]
    public struct DamageModifier
    {
        public ModifierScope scope;
        public ModifierType type;
        public float value;
        
        public bool filterByHitType;
        public HitTypes hitTypeses;
        
        //public ElementTypes elementTypes;
        //public DamageTypes damageTypes;

        //public bool filterByElement;
        //public bool filterByDamageType;

        public bool AppliesTo(ModifierScope actualScope, HitTypes? actualHitType)
        {
            // Scope check
            bool scopeMatch =
                scope == ModifierScope.AllDamage ||
                scope == actualScope;

            if (!scopeMatch)
                return false;

            // HitType check
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
            
            if (!actualHitType.HasValue)
                return false;

            return actualHitType.HasValue && (actualHitType.Value & hitTypeses) != 0; // Check the HitType flags.
        }
    }
}