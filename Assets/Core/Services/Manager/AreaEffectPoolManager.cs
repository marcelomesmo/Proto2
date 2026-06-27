using System.Collections.Generic;
using Core.Gameplay.Combat.AreaAttack;
using Core.Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Services.Manager
{
    public class AreaEffectPoolManager : MonoBehaviour, IPool
    {
        public static AreaEffectPoolManager Instance { get; private set; }

        [System.Serializable]
        public class AreaEffectPool
        {
            public GameObject areaPrefab;
            public bool collectionCheck = true;
            public int defaultCapacity = 20;
            public int maxSize = 100;
        }

        [Header("AreaEffect Pools")]
        [Tooltip("List of pools with the area prefabs")]
        public List<AreaEffectPool> areaPools;

        // Manage all available pools, located by projectile type (gameObject)
        private readonly Dictionary<GameObject, IObjectPool<AreaAttackInstance>> _allPools = new();
        private Dictionary<GameObject, HashSet<AreaAttackInstance>> _activeObjects = new();

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
            foreach (var pool in areaPools)
            {
                var prefab = pool.areaPrefab;
                var prefabController = prefab.GetComponent<AreaAttackInstance>();
            
                if (prefabController == null)
                {
                    Debug.LogError($"Prefab {prefab.name} has no BaseAreaEffect component!");
                    continue;
                }
                
                _activeObjects.Add(prefab, new HashSet<AreaAttackInstance>());

                IObjectPool<AreaAttackInstance> objectPool = null;  // IMPORTANT: create pool variable first so createFunc can reference it
            
                objectPool = new ObjectPool<AreaAttackInstance>(
                    createFunc: () =>
                    {
                        var objGO = Instantiate(prefab, transform);
                        var obj = objGO.GetComponent<AreaAttackInstance>();
                    
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
                        obj.transform.SetParent(transform);
                        
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
        public AreaAttackInstance Spawn(GameObject prefab)
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
                IObjectPool<AreaAttackInstance> pool = _allPools[prefab];

                // Copy to avoid modifying collection while iterating
                var snapshot = ListPool<AreaAttackInstance>.Get();
                snapshot.AddRange(kvp.Value);

                foreach (var obj in snapshot)
                {
                    pool.Release(obj);
                }

                kvp.Value.Clear();
                ListPool<AreaAttackInstance>.Release(snapshot);
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