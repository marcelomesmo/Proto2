using System.Collections.Generic;
using Game.Entity.Player;
using UnityEngine;

namespace Core.Gameplay.Spawner
{
    [CreateAssetMenu(fileName = "Entity Spawner Data", menuName = "Spawner/Entity Spawner Data")]
    public class EntitySpawnerData : ScriptableObject
    {
        [System.Serializable]
        public class WaveDefinition
        {
            [Header("Entities")]
            public List<CharacterDefinition> entities = new();

            // TODO: Later add events, this variable are not used for now but will be important for: UI banners, analytics, achievements, unlocks, music transitions.
            [Header("Presentation")]
            public string waveId;
            public bool isBossWave;
            // TODO: Later add wave start sfx/vfx here.
            //public AudioClip introSfx;
            //public GameObject introVfx;
            
            [Header("Timing")]
            [Tooltip("Optional override. < 0 uses spawner default.")]
            public float overrideWaveDuration = -1f;
            [Tooltip("Optional override. Lock wave, ignores duration.")]
            public bool killAllRequired;
        }
        
        [Header("Spawn Timing")]
        [Tooltip("Delay between spawns inside a wave.")]
        public float spawnDelay = 2f;
        public float minSpawnRadius = 3f;
        
        [Tooltip("Default delay between waves.")]
        public float defaultWaveDuration = 5f;
        
        [Header("Waves")]
        public List<WaveDefinition> waves = new();
        
        public int TotalWaveCount => waves?.Count ?? 0;
    }
}
