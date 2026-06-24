using System.Collections.Generic;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Spawn;
using Core.Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Services.Manager
{
    public class EntityPoolManager : MonoBehaviour, IPool
    {
        public static EntityPoolManager Instance { get; private set; }
        
        [System.Serializable]
        public class EnemyPool
        {
            public EntityController prefab;
            public bool collectionCheck = true;
            public int defaultCapacity = 20;
            public int maxSize = 100;
        }
        
        [Header("Entity Pools")]
        [Tooltip("List of pools with the entity prefabs")]
        public List<EnemyPool> entityPools;

        // Manage all available pools, located by projectile type (gameObject)
        private readonly Dictionary<EntityController, IObjectPool<EntityController>> _allPools = new();
        private readonly Dictionary<EntityController, HashSet<EntityController>> _activeObjects = new();

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
            foreach (var entry in entityPools)
            {
                var prefab = entry.prefab;
                
                var prefabController = prefab.GetComponent<EntityController>();
                if (prefabController == null)
                {
                    Debug.LogError($"Prefab {prefab.name} has no EntityController component!");
                    continue;
                }
                
                // 1. Initialize tracking FIRST
                _activeObjects.Add(prefab, new HashSet<EntityController>());
                
                IObjectPool<EntityController> pool = null;  // IMPORTANT: create pool variable first so createFunc can reference it
                
                // 2. Create pool
                pool = new ObjectPool<EntityController>(
                    createFunc: () =>
                    {
                        var obj = Instantiate(prefab, transform);
                        //var obj = objGO.GetComponent<EntityController>();
                        obj.AssignToPool(pool);    // ← gives obj its pool reference
                        obj.gameObject.SetActive(false);
                        return obj;
                    },
                    actionOnGet: obj =>
                    {
                        Debug.Assert(!obj.IsReleased,
                            $"[{obj.name}] Retrieved from pool but marked released.");
                        
                        obj.gameObject.SetActive(true);
                        
                        _activeObjectCount++;
                        _activeObjects[prefab].Add(obj);
                        
                        // DO NOT call Configure or OnSpawn here. Now handled over Spawn().
                    },
                    actionOnRelease: obj =>
                    {
                        obj.gameObject.SetActive(false);
                        obj.OnDespawn();
                        
                        _activeObjectCount--;
                        _activeObjects[prefab].Remove(obj);
                    },
                    actionOnDestroy: o => Destroy(o.gameObject),
                    collectionCheck: entry.collectionCheck,
                    defaultCapacity: entry.defaultCapacity,
                    maxSize: entry.maxSize
                );
                
                // 3. Register pool LAST
                _allPools.Add(prefab, pool);
            }
        }
         
        // Called by Spawner or PartyController or GameController to create new entities.
        public EntityController Spawn(EntityController prefab, Vector3 position, Quaternion rotation, in SpawnContext context)
        {
            if (!_allPools.TryGetValue(prefab, out var pool))
            {
                Debug.LogError($"No pool exists for prefab: {prefab.name}");
                return null;
            }
            
            var entity = pool.Get();
            
            entity.transform.SetPositionAndRotation(position, rotation);

            entity.Configure(context);  // <- inject data
            entity.OnSpawn();           // <- initialize subsystems

            return entity;
        }
        
        public void Despawn(EntityController entity)
        {
            if (entity == null)
                return;

            if (entity.IsReleased)
                return;

            //entity.OnDespawn();   was being called twice.

            if (entity.TryGetAssignedPool(out var pool))
                pool.Release(entity);
            else
                Destroy(entity.gameObject);
        }
        
        public void ReleaseAll()
        {
            foreach (var kvp in _activeObjects)
            {
                EntityController prefab = kvp.Key;
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
        
        public void Clear()
        {
            // Release all active instances first
            ReleaseAll();

            // Destroy all inactive pooled instances
            foreach (var kvp in _allPools)
                kvp.Value.Clear(); // ObjectPool.Clear() destroys all inactive objects via actionOnDestroy
        }
    }
}