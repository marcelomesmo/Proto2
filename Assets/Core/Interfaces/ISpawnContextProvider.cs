using Core.Gameplay.Entity.Spawn;
using Game.Entity.Player;
using UnityEngine;

namespace Core.Interfaces
{
    public interface ISpawnContextProvider
    {
        SpawnContext CreateContext(
            CharacterDefinition definition,
            GameObject owner,
            int waveIndex,
            int spawnIndex
        );
    }
}