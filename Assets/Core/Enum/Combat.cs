namespace Core.Enum
{
    [System.Flags]
    public enum HitTypes
    {
        None = 0,
        Physical = 1 << 0,
        Slash = 1 << 1,
        Fire = 1 << 2,
        Ice = 1 << 3,
        Poison = 1 << 4,
        Shock = 1 << 5,
        Explosive = 1 << 6,
    }
    
    /*
     * Later, in case we want to differentiate between Element/Damage types.
     
        [System.Flags]
        public enum ElementType
        {
            None  = 0,
            Fire  = 1 << 0,
            Ice   = 1 << 1,
            Earth = 1 << 2,
            Poison = 1 << 3,
        }

        [System.Flags]
        public enum DamageType
        {
            None      = 0,
            Slash     = 1 << 0,
            Pierce    = 1 << 1,
            Blunt     = 1 << 2,
            Projectile= 1 << 3,
            AoE       = 1 << 4,
        }
    */
}