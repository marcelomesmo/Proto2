using System.Collections.Generic;
using Game.Entity.Player;
using UnityEngine;

namespace Core.Gameplay.Spawner
{
    [CreateAssetMenu(fileName = "Entity Spawner Data", menuName = "Spawner/Entity Spawner Data")]
    public class EntitySpawnerData : ScriptableObject
    {
        [System.Serializable]
        public class SpawnWave
        {
            public List<CharacterDefinition> entities;
        }
        
        public float spawnDelay = 2f;
        public float minSpawnRadius = 3f;

        public List<SpawnWave> spawnList = new();
        public float waveDelay = 5f;
    }
}
