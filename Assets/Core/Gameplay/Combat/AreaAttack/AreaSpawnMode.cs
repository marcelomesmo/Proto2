namespace Core.Gameplay.Combat.AreaAttack
{
    public enum AreaSpawnMode
    {
        WorldPosition,     // current behavior (ground AoE)
        AtCaster,          // centered on entity
        InFrontOfCaster,   // frontal (flamethrower)
    }
}