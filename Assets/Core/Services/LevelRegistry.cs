using System.Collections.Generic;
using Core.Level;
using UnityEngine;

namespace Core.Services
{
    [CreateAssetMenu(fileName = "LevelRegistry", menuName = "Levels/Level Registry")]
    public class LevelRegistry : ScriptableObject
    {
        [Header("Level List")]
        [SerializeField] private LevelConfig[] entries;

        private Dictionary<string, LevelConfig> _index;
        
        public void BuildIndex()
        {
            _index = new Dictionary<string, LevelConfig>(entries.Length);

            foreach (var config in entries)
            {
                if (config == null || string.IsNullOrEmpty(config.SceneName))
                {
                    Debug.LogWarning("[LevelRegistry] Skipping invalid or incomplete LevelConfig.");
                    continue;
                }

                if (!_index.TryAdd(config.SceneName, config))
                    Debug.LogWarning($"[LevelRegistry] Duplicate scene name: {config.SceneName}");
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
