namespace Gameplay.Projectile
{
    public readonly struct ProjectileContext
    {
        public readonly Faction faction;
        public readonly float range;
        public readonly int bonusPierce;
        public readonly float damageMultiplier;
        // future modifiers

        public ProjectileContext( 
            Faction faction,
            float range,
            int bonusPierce,
            float damageMultiplier)
        {
            this.faction = faction;
            this.range = range;
            this.bonusPierce = bonusPierce;
            this.damageMultiplier = damageMultiplier;
        }
    }
}