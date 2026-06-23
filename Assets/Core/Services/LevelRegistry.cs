using System;
using System.Collections.Generic;
using Core.Level;
using UnityEngine;

namespace Core.Services
{
    [CreateAssetMenu(fileName = "LevelRegistry", menuName = "Levels/Level Registry")]
    public class LevelRegistry : ScriptableObject
    {
        [Serializable]
        private struct LevelEntry
        {
            public string sceneName;
            public LevelConfig config;
        }
        
        [Header("Level List")]
        [SerializeField] private LevelEntry[] entries;

        private Dictionary<string, LevelConfig> _index;
        
        public void BuildIndex()
        {
            _index = new Dictionary<string, LevelConfig>(entries.Length);

            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.sceneName) || entry.config == null)
                {
                    Debug.LogWarning("[LevelRegistry] Skipping invalid entry.");
                    continue;
                }

                if (!_index.TryAdd(entry.sceneName, entry.config))
                    Debug.LogWarning($"[LevelRegistry] Duplicate scene name: {entry.sceneName}");
            }
        }

        public bool TryGet(string sceneName, out LevelConfig config)
        {
            if (_index == null)
            {
                Debug.LogError("[LevelRegistry] Index not built. Call Initialize() first.");
                config = null;
                return false;
            }

            return _index.TryGetValue(sceneName, out config);
        }
    }
}
