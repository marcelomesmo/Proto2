namespace Core.Gameplay.Combat.StatusEffect
{
    // Runtime-only damage descriptor.
    // Used for DOT, environment damage, etc.
    public readonly struct DamageDefinition
    {
        public readonly int damage;

        public DamageDefinition(int damage)
        {
            this.damage = damage;
        }
    }
}