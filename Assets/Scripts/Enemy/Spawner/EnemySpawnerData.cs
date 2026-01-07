using System.Collections.Generic;
using UnityEngine;

namespace Enemy.Spawner
{
    [CreateAssetMenu(fileName = "EnemySpawnerData", menuName = "Spawner/Enemy Spawner Data")]
    public class EnemySpawnerData : ScriptableObject
    {
        [System.Serializable]
        public class EnemyList
        {
            public List<GameObject> enemyPrefabs;
        }
        
        public float spawnDelay = 2f;
        public float minSpawnRadius = 3f;
        public float slotSpacing = 1.5f;

        public List<EnemyList> spawnList = new();
        public float waveDelay = 5f;
    }
}
