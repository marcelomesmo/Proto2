using Core.Gameplay.Entity.Spawner;
using Core.Interfaces;
using Game.Entity.Player;
using UnityEngine;

namespace Game.Spawner
{
    public class DefaultSpawnContextProvider : MonoBehaviour, ISpawnContextProvider
    {
        public SpawnContext CreateContext(
            CharacterDefinition definition,
            GameObject owner,
            int waveIndex,
            int spawnIndex
        )
        {
            int level = 1 + waveIndex;
            float powerMultiplier = 1f;

            return new SpawnContext(
                stats: definition.baseStats,
                attackLoadout: definition.initialAttackLoadout,
                owner: owner,
                level: level,
                powerMultiplier: powerMultiplier
            );
        }
    }
}
