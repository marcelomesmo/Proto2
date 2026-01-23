using System.Collections.Generic;
using Core.Gameplay.Combat.Projectile;
using Core.Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Services.Manager
{
    public class ProjectilePoolManager : MonoBehaviour, IPool
    {
        public static ProjectilePoolManager Instance { get; private set; }

        [System.Serializable]
        public class ProjectilePool
        {
            public GameObject projectilePrefab;
            public bool collectionCheck = true;
            public int defaultCapacity = 20;
            public int maxSize = 100;
        }

        [Header("Projectile Pools")]
        [Tooltip("List of pools with the projectile prefabs")]
        public List<ProjectilePool> projectilePools;

        // Manage all available pools, located by projectile type (gameObject)
        private readonly Dictionary<GameObject, IObjectPool<ProjectileInstance>> _allPools = new();
        private Dictionary<GameObject, HashSet<ProjectileInstance>> _activeObjects = new();

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
            foreach (var pool in projectilePools)
            {
                var prefab = pool.projectilePrefab;
                var prefabController = prefab.GetComponent<ProjectileInstance>();
            
                if (prefabController == null)
                {
                    Debug.LogError($"Prefab {prefab.name} has no BaseProjectile component!");
                    continue;
                }
                
                _activeObjects.Add(prefab, new HashSet<ProjectileInstance>());

                IObjectPool<ProjectileInstance> objectPool = null;  // IMPORTANT: create pool variable first so createFunc can reference it
            
                objectPool = new ObjectPool<ProjectileInstance>(
                    createFunc: () =>
                    {
                        var objGO = Instantiate(prefab, transform);
                        var obj = objGO.GetComponent<ProjectileInstance>();
                    
                        obj.AssignToPool(objectPool);    // ← gives projectile its pool reference
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
            
                _allPools.Add(prefab, objectPool);
            }
        }
    
        // Called by WeaponManager to spawn new Projectiles.
        public ProjectileInstance Spawn(GameObject prefab)
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
                IObjectPool<ProjectileInstance> pool = _allPools[prefab];

                // Copy to avoid modifying collection while iterating
                var snapshot = ListPool<ProjectileInstance>.Get();
                snapshot.AddRange(kvp.Value);

                foreach (var obj in snapshot)
                {
                    pool.Release(obj);
                }

                kvp.Value.Clear();
                ListPool<ProjectileInstance>.Release(snapshot);
            }
        }
    
        #region Util Pool Checks
        public int GetPoolSize(GameObject prefab)
        {
            if (_allPools.ContainsKey(prefab)) return 0; // TODO: Figure out how to get the pool.capacity.
            Debug.Log($"[ProjectilePoolManager] No pool exists for prefab: {prefab.name}");
            return 0;
        }
        
        public int GetPoolIdle(GameObject prefab)
        {
            if (_allPools.TryGetValue(prefab, out var pool)) return pool.CountInactive;
            Debug.Log($"[ProjectilePoolManager] No pool exists for prefab: {prefab.name}");
            return 0;
        }

        public int GetPoolActive()
        {
            return _activeObjectCount;
        }
        #endregion
    }
}