using System.Collections.Generic;
using Core.Interfaces;
using Core.VFX;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Services.Manager
{
    public class VFXPoolManager : MonoBehaviour, IPool
    {
        public static VFXPoolManager Instance { get; private set; }
    
        [System.Serializable]
        public class VFXPoolEntry
        {
            public PooledVFX  prefab;
            public bool collectionCheck = true;
            public int defaultCapacity = 20;
            public int maxSize = 100;
        }
        [Header("VFX Pools")]
        [Tooltip("List of pools with the vfx prefabs")]
        public List<VFXPoolEntry> vfxPools;
    
        private readonly Dictionary<PooledVFX, IObjectPool<PooledVFX>> _allPools = new();
        private Dictionary<PooledVFX, HashSet<PooledVFX>> _activeObjects = new();
    
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
            foreach (var pool in vfxPools)
            {
                var prefab = pool.prefab;

                if (prefab == null)
                {
                    Debug.LogWarning("[VFXPoolManager] Null prefab entry skipped.");
                    continue;
                }

                _activeObjects.Add(prefab, new HashSet<PooledVFX>());

                IObjectPool<PooledVFX> objPool = null;

                objPool = new ObjectPool<PooledVFX>(
                    createFunc: () =>
                    {
                        var instance = Instantiate(prefab, transform);
                        instance.AssignToPool(objPool);
                        instance.gameObject.SetActive(false);
                        return instance;
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
                    actionOnDestroy: obj => Destroy(obj.gameObject),
                    collectionCheck: pool.collectionCheck,
                    defaultCapacity: pool.defaultCapacity,
                    maxSize: pool.maxSize
                );

                _allPools.Add(prefab, objPool);
            }
        }

        public PooledVFX Spawn(PooledVFX prefab, Vector3 position, Quaternion rotation)
        {
            if (!_allPools.TryGetValue(prefab, out var pool))
            {
                Debug.LogError($"[VFXPoolManager] No pool for prefab: {prefab.name}");
                return null;
            }

            var instance = pool.Get();
            instance.transform.SetPositionAndRotation(position, rotation);
            return instance;
        }
    
        public void ReleaseAll()
        {
            foreach (var kvp in _activeObjects)
            {
                var prefab = kvp.Key;
                var pool = _allPools[prefab];

                // Copy to avoid modifying collection while iterating
                var snapshot = ListPool<PooledVFX>.Get();
                snapshot.AddRange(kvp.Value);

                foreach (var obj in snapshot)
                {
                    if (!obj) continue; // <--- guard
                    pool.Release(obj);
                }

                kvp.Value.Clear();
                ListPool<PooledVFX>.Release(snapshot);
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
        public int GetPoolActive() => _activeObjectCount;

        public int GetPoolIdle(PooledVFX prefab)
        {
            if (_allPools.TryGetValue(prefab, out var pool))
                return pool.CountInactive;

            Debug.LogWarning($"[VFXPoolManager] No pool for prefab: {prefab.name}");
            return 0;
        }
        #endregion
    }
}