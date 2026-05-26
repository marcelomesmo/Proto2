using Core.Interfaces;
using Core.Services.Manager;
using Game.Entity.Player;
using UnityEngine;

namespace Core.Gameplay.Spawner
{
    /*
     * Only responsible for spawning entities.
     * No wave progression, no timers, no runtime flow.
     *
     * Controlled externally by EntitySpawnerRuntimeController.
     */
    public abstract class EntitySpawner : MonoBehaviour
    {
        [Header("Spawn Context")]
        [SerializeField, Tooltip("REQUIRED. Must implement ISpawnContextProvider.")] 
        private MonoBehaviour spawnContextProviderBehaviour;
        
        // --------------------------------------------------
        // Spawn Context
        // --------------------------------------------------
        
        private ISpawnContextProvider _contextProvider;
        
        // --------------------------------------------------
        // Spawn Slots
        // --------------------------------------------------

        protected Vector2[] SpawnSlots;
        protected int[] SlotOrder;   // helper to shuffle slot spawn order
        
        // --------------------------------------------------
        // RuntimeController report
        // --------------------------------------------------
        
        private EntitySpawnerRuntimeController _runtimeController;
        
        // Lazy-getter to allow gizmo fetching runtimecontroller in editor
        protected EntitySpawnerRuntimeController RuntimeController
        {
            get
            {
                if (_runtimeController == null)
                    _runtimeController =
                        GetComponent<EntitySpawnerRuntimeController>();

                return _runtimeController;
            }
        }
        
        // --------------------------------------------------
        // Unity Lifecycle
        // --------------------------------------------------
        
        protected virtual void Awake()
        {
            if (RuntimeController == null)
            {
                throw new MissingComponentException(
                    $"{name}: Missing EntitySpawnerRuntimeController."
                );
            }

            _contextProvider = 
                spawnContextProviderBehaviour as ISpawnContextProvider;
            
            if (_contextProvider == null)
                throw new MissingComponentException(
                    $"{name}: EntitySpawner requires a component implementing ISpawnContextProvider."
                );
        }
        
        // --------------------------------------------------
        // Spawn Control
        // --------------------------------------------------

        public virtual void PrepareWave()
        {
            BuildSpawnSlots();
            BuildSlotOrder();
        }
        
        public void SpawnWaveEntity(
            EntitySpawnerData.WaveDefinition wave,
            int currentWaveIndex,
            int spawnIndex)
        {
            if (wave == null)
                return;

            if (spawnIndex < 0 || spawnIndex >= wave.entities.Count)
                return;

            SpawnEntity(
                wave.entities[spawnIndex],
                currentWaveIndex,
                wave.entities.Count,
                spawnIndex
            );
        }

        // --------------------------------------------------
        // Spawn Execution
        // --------------------------------------------------
        
        private void SpawnEntity(
            CharacterDefinition character, 
            int currentWaveIndex,
            int waveSize, 
            int spawnIndex)
        {
            Vector2 offset = GetSpawnSlotOffset(spawnIndex);

            Vector2 position = (Vector2)transform.position + offset;

            var context = _contextProvider.CreateContext(
                character,
                gameObject,
                currentWaveIndex,
                spawnIndex
            );

            var entity = EntityPoolManager.Instance.Spawn(
                character.prefab,
                position,
                Quaternion.identity,
                context
            );

            _runtimeController.RegisterSpawnedEnemy(entity);
            
            // Intentionally no post-spawn logic yet
        }
        
        /*
            Can adapt based on spawn pattern:
            - Horizontal spawners → (x, 0)
            - Lane spawners → (0, y)
            - Radial spawners → (cosθ, sinθ) * radius
            etc
         */
        protected Vector2 GetSpawnSlotOffset(int spawnIndex)
        {
            if (SpawnSlots == null || SpawnSlots.Length == 0)
                return Vector2.zero;
            
            // defensive guard
            if (SlotOrder == null || SlotOrder.Length == 0)
                return SpawnSlots[spawnIndex % SpawnSlots.Length];

            int slotIndex = SlotOrder[spawnIndex % SlotOrder.Length];
            return SpawnSlots[slotIndex];
        }
        
        // --------------------------------------------------
        // Slot Construction
        // --------------------------------------------------
        
        // Can be built different ways: Lane, Horizontal, 8dir.
        protected abstract void BuildSpawnSlots();
        
        protected abstract void BuildSlotOrder();

        protected int GetLargestWaveSize()
        {
            if (_runtimeController.SpawnData == null)
                return 0;

            int max = 0;

            foreach (var wave in _runtimeController.SpawnData.waves)
            {
                if (wave.entities.Count > max)
                    max = wave.entities.Count;
            }

            return max;
        }
        
#if UNITY_EDITOR
        
        [Header("### Editor Preview ###")]
        [SerializeField, Tooltip("Editor-only: which wave to preview. Used by implementations.")]
        protected int previewWaveIndex = 0;

        [SerializeField, Tooltip("Editor-only: preview only first N spawns (0 = all). Used by implementations.")]
        protected int previewSpawnCount = 0;
        
        protected virtual void OnDrawGizmosSelected()
        {
            var slots = GetGizmoSlotPositions();
            if (slots == null)
                return;

            Gizmos.color = Color.yellow;

            int slotNumber = 0;
            foreach (var pos in slots)
            {
                Gizmos.DrawWireSphere(pos, 0.1f);
                Gizmos.DrawLine(transform.position, pos);
                
                // Label slot index (Scene view only)
                slotNumber++;
                UnityEditor.Handles.Label(
                    pos + Vector3.up * 0.3f,
                    $"Slot {slotNumber}"
                );
            }
        }
        
        protected abstract Vector3[] GetGizmoSlotPositions();

        protected abstract Vector3[] GetPreviewSpawnPositions();
        
        protected virtual void OnValidate()
        {
            if (spawnContextProviderBehaviour == null)
                return;

            if (!(spawnContextProviderBehaviour is ISpawnContextProvider))
            {
                Debug.LogError(
                    $"{name}: Assigned SpawnContextProvider does not implement ISpawnContextProvider.",
                    this
                );
                spawnContextProviderBehaviour = null;
            }
        }
#endif
    }
}