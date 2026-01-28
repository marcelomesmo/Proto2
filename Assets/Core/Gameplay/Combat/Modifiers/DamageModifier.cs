using System;

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
        Effect
    }
    
    [Serializable]
    public struct DamageModifier
    {
        public ModifierScope scope;
        public ModifierType type;
        public float value;
    }
}