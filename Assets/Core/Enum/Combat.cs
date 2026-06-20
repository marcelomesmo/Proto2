namespace Core.Enum
{
    [System.Flags]
    public enum HitTypes
    {
        None = 0,
        Physical = 1 << 0,
        Magical = 1 << 1,
        Fire = 1 << 2,
        Water = 1 << 3,
        Grass = 1 << 4,
        Electric = 1 << 5,
        Explosive = 1 << 6,
    }
    
    public enum ModifierType
    {
        Additive,           // +10 damage
        Multiplicative      // x1.2 damage
    }

    public enum ModifierScope
    {
        All,
        Melee,
        Projectile,
        Area,
        Direct,
        Chain,
        Effect
    }
    
    public enum AttackStatType
    {
        Damage,
        Healing,
        Cooldown,
        Duration,
        Range,
        
        ExtraExecutions,
        ExtraChainBounces,

        // future
        //ChainRange,
        ChainDamageMultiplier,
        //ProjectilePierce
    }

	public enum AttackVariantCondition
	{	
		EveryNthCast,
		RandomChance,
	}

    public enum CombatAction
    {
        Damage,
        Heal,
        ApplyStatus,
        RemoveStatus,
        ModifyStat,
        Revive
    }

    public enum CombatTargetType
    {
        Self,
        Enemies,
        Allies,
        Any
    }

    public enum TargetSelectionMode
    {
        Closest,
        Furthest,
        LowestHealth,
        HighestHealth
    }
    
    public enum TargetRequirement
    {
        None,
        MissingHealth,
        MissingShield,
        HasDebuff,
        Dead
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