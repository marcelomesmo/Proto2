using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Core.Gameplay.Entity.Spawn
{
    public readonly struct SpawnContext
    {
        public readonly BaseEntityStats Stats;
        public readonly AttackLoadoutDefinition AttackLoadout;
        public readonly GameObject Owner;          // Optional (player, spawner, system)
        public readonly int Level;                 // Optional, but future-proof
        public readonly float PowerMultiplier;     // For buffs / scaling

        public SpawnContext(
            BaseEntityStats stats,
            AttackLoadoutDefinition attackLoadout = null,
            GameObject owner = null,
            int level = 1,
            float powerMultiplier = 1f)
        {
            Stats = stats;
            AttackLoadout = attackLoadout;
            Owner = owner;
            Level = level;
            PowerMultiplier = powerMultiplier;
        }
    }
}