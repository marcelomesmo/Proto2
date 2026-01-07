using System.Collections.Generic;
using Core.Gameplay.Entity;
using Core.Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Services.Manager
{
    public class EnemyPoolManager : MonoBehaviour, IPool
    {
        public static EnemyPoolManager Instance { get; private set; }
        
        [System.Serializable]
        public class EnemyPool
        {
            public GameObject enemyPrefab;
            public bool collectionCheck = true;
            public int defaultCapacity = 20;
            public int maxSize = 100;
        }
        
        [Header("Enemy Pools")]
        [Tooltip("List of pools with the enemy prefabs")]
        public List<EnemyPool> enemyPools;

        // Manage all available pools, located by projectile type (gameObject)
        private readonly Dictionary<GameObject, IObjectPool<EntityController>> _allPools = new();
        private Dictionary<GameObject, HashSet<EntityController>> _activeObjects = new();

        private int _activeObjectCount; // Custom counter for active objects

        private void Awake()
        {
            // Simple Singleton
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            InitializePools();
        }

        private void InitializePools()
        {
            foreach (var pool in enemyPools)
            {
                var prefab = pool.enemyPrefab;
                var prefabController = prefab.GetComponent<EntityController>();
                
                if (prefabController == null)
                {
                    Debug.LogError($"Prefab {prefab.name} has no BaseEnemyController component!");
                    continue;
                }
                
                // 1. Initialize tracking FIRST
                _activeObjects.Add(prefab, new HashSet<EntityController>());
                
                IObjectPool<EntityController> objectPool = null;  // IMPORTANT: create pool variable first so createFunc can reference it
                
                // 2. Create pool
                objectPool = new ObjectPool<EntityController>(
                    createFunc: () =>
                    {
                        var objGO = Instantiate(prefab, transform);
                        var obj = objGO.GetComponent<EntityController>();
                        
                        obj.AssignToPool(objectPool);    // ← gives obj its pool reference
                        obj.gameObject.SetActive(false);
                        return obj;
                    },
                    actionOnGet: obj =>
                    {
                        obj.gameObject.SetActive(true);
                        obj.OnSpawn();
                        
                        _activeObjectCount++;
                        _activeObjects[prefab].Add(obj);
                    },
                    actionOnRelease: obj =>
                    {
                        obj.gameObject.SetActive(false);
                        obj.OnDespawn();
                        
                        _activeObjectCount--;
                        _activeObjects[prefab].Remove(obj);
                    },
                    actionOnDestroy: o => Destroy(o.gameObject),
                    collectionCheck: pool.collectionCheck,
                    defaultCapacity: pool.defaultCapacity,
                    maxSize: pool.maxSize
                );
                
                // 3. Register pool LAST
                _allPools.Add(prefab, objectPool);
            }
        }
         
        // Called by Spawner to spawn new Enemies.
        public EntityController Spawn(GameObject prefab)
        {
            if (!_allPools.TryGetValue(prefab, out var pool))
            {
                Debug.LogError($"No pool exists for prefab: {prefab.name}");
                return null;
            }

            return pool.Get();
        }
        
        public void ReleaseAll()
        {
            foreach (var kvp in _activeObjects)
            {
                GameObject prefab = kvp.Key;
                IObjectPool<EntityController> pool = _allPools[prefab];

                // Copy to avoid modifying collection while iterating
                var snapshot = ListPool<EntityController>.Get();
                snapshot.AddRange(kvp.Value);

                foreach (var obj in snapshot)
                {
                    pool.Release(obj);
                }

                kvp.Value.Clear();
                ListPool<EntityController>.Release(snapshot);
            }
        }

        #region Util Pool Checks
        public int GetPoolSize(GameObject prefab)
        {
            if (_allPools.ContainsKey(prefab)) return 0; // TODO: Figure out how to get the pool.capacity.
            Debug.Log($"[EnemyPoolManager] No pool exists for prefab: {prefab.name}");
            return 0;
        }
        
        public int GetPoolIdle(GameObject prefab)
        {
            if (_allPools.TryGetValue(prefab, out var pool)) return pool.CountInactive;
            Debug.Log($"[EnemyPoolManager] No pool exists for prefab: {prefab.name}");
            return 0;
        }

        public int GetPoolActive()
        {
            return _activeObjectCount;
        }
        #endregion
    }
}