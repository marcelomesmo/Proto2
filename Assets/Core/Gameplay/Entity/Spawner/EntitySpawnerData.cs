using System.Collections.Generic;
using UnityEngine;

namespace Core.Gameplay.Entity.Spawner
{
    [CreateAssetMenu(fileName = "Entity Spawner Data", menuName = "Spawner/Entity Spawner Data")]
    public class EntitySpawnerData : ScriptableObject
    {
        [System.Serializable]
        public class EntityList
        {
            public List<EntityController> prefabs;
        }
        
        public float spawnDelay = 2f;
        public float minSpawnRadius = 3f;
        public float slotSpacing = 1.5f;

        public List<EntityList> spawnList = new();
        public float waveDelay = 5f;
    }
}
