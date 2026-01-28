using System.Collections.Generic;
using Core.Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Services.Manager
{
    public class VFXPoolManager : MonoBehaviour, IPool
    {
        public static VFXPoolManager Instance { get; private set; }
    
        [System.Serializable]
        public class VFXPool
        {
            public GameObject vfxPrefab;
            public bool collectionCheck = true;
            public int defaultCapacity = 20;
            public int maxSize = 100;
        }
        [Header("VFX Pools")]
        [Tooltip("List of pools with the vfx prefabs")]
        public List<VFXPool> vfxPools;
    
        private readonly Dictionary<GameObject, IObjectPool<GameObject>> _allPools = new();
        private Dictionary<GameObject, HashSet<GameObject>> _activeObjects = new();
    
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
        }

        public GameObject Spawn(GameObject prefab)
        {
            if (!_allPools.TryGetValue(prefab, out var pool))
            {
                _activeObjects.Add(prefab, new HashSet<GameObject>());
                
                pool = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(prefab, transform),
                    actionOnGet: obj =>
                    {
                        obj.SetActive(true);
                        
                        _activeObjectCount++;
                        _activeObjects[prefab].Add(obj);
                    },
                    actionOnRelease: obj =>
                    {
                        if (!obj) return; // <--- CRITICAL GUARD
                        
                        obj.SetActive(false);
                        
                        _activeObjectCount--;
                        _activeObjects[prefab].Remove(obj);
                    });

                _allPools[prefab] = pool;
            }

            //var vfx = pool.Get();
            //StartCoroutine(ReleaseAfter(vfx, 0.5f, pool)); // auto-return
            //return vfx;
            return pool.Get();
        }

        /*private IEnumerator ReleaseAfter(GameObject obj, float delay, IObjectPool<GameObject> pool)
    {
        yield return new WaitForSeconds(delay);
        pool.Release(obj);
    }*/

        public void Release(GameObject prefab, GameObject instance)
        {
            if (!_allPools.TryGetValue(prefab, out var pool))
                return;

            if (!_activeObjects.TryGetValue(prefab, out var activeSet))
                return;

            if (!activeSet.Contains(instance))
                return; // already released

            pool.Release(instance);
        }
    
        public void ReleaseAll()
        {
            foreach (var kvp in _activeObjects)
            {
                GameObject prefab = kvp.Key;
                IObjectPool<GameObject> pool = _allPools[prefab];

                // Copy to avoid modifying collection while iterating
                var snapshot = ListPool<GameObject>.Get();
                snapshot.AddRange(kvp.Value);

                foreach (var obj in snapshot)
                {
                    if (!obj) continue; // <--- guard
                    pool.Release(obj);
                }

                kvp.Value.Clear();
                ListPool<GameObject>.Release(snapshot);
            }
        }
        
        #region Util Pool Checks
        public int GetPoolSize(GameObject prefab)
        {
            if (_allPools.ContainsKey(prefab)) return 0; // TODO: Figure out how to get the pool.capacity.
            Debug.Log($"[VFXPoolManager] No pool exists for prefab: {prefab.name}");
            return 0;
        }
        
        public int GetPoolIdle(GameObject prefab)
        {
            if (_allPools.TryGetValue(prefab, out var pool)) return pool.CountInactive;
            Debug.Log($"[VFXPoolManager] No pool exists for prefab: {prefab.name}");
            return 0;
        }

        public int GetPoolActive()
        {
            return _activeObjectCount;
        }
        #endregion
    }
}