using System;
using Core.Interfaces;
using Core.Services.Manager;
using Core.Util;
using Game.Entity.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Gameplay.Spawner
{
    public abstract class EntitySpawner : MonoBehaviour
    {
        [Header("Spawn Data")]
        [SerializeField] protected EntitySpawnerData spawnData;
        [SerializeField, Tooltip("REQUIRED. Must implement ISpawnContextProvider.")] 
        private MonoBehaviour spawnContextProviderBehaviour;

        [Header("Trigger Settings")]
        [SerializeField] private bool spawnOnGameStart;
        [SerializeField] private float spawnStartDelay;
        
        [Header("Proximity Trigger")]
        [SerializeField] private bool useProximityTrigger = false;
        [SerializeField] private float triggerRadius = 5f;
        
        [Header("Refresh Settings")]
        [Tooltip("Time for the spawner to be available again (restart) after current set of waves is completed. 0 = no refresh.")]
        [SerializeField] private float restartDelay = 0f;
        
        // --------------------------------------------------
        // Internal State
        // --------------------------------------------------
        private SpawnerState _state = SpawnerState.Waiting;
        
        private CooldownTimer _startDelayTimer;
        private CooldownTimer _refreshTimer;
        
        public bool IsSpawning => _state == SpawnerState.Spawning;
        
        // Spawn context
        private ISpawnContextProvider _contextProvider;
        
        // Wave progression
        private int _currentWaveIndex;
        private int _spawnedElementsInWave;
        private bool _waveFinished;
        
        // Timers
        private float _spawnTimer;
        private float _waveCooldownTimer;
        
        // Spawn slots
        protected Vector2[] SpawnSlots;
        protected int[] SlotOrder;   // helper to shuffle slot spawn order
        
        // Proximity specific
        private bool _proximityTriggered;
        
        public event Action OnWaveCompletedSignal;
        public event Action OnAllWavesCompletedSignal;
        
        // --------------------------------------------------
        // Unity Lifecycle
        // --------------------------------------------------
        
        protected virtual void Awake()
        {
            _startDelayTimer = new CooldownTimer();
            _refreshTimer = new CooldownTimer();
            
            _contextProvider = spawnContextProviderBehaviour as ISpawnContextProvider;
            if (_contextProvider == null)
                throw new MissingComponentException(
                    $"{name}: EntitySpawner requires a component implementing ISpawnContextProvider."
                );
        }
        
        protected virtual void Start()
        {
            ResetSpawnerInternal();

            if (spawnOnGameStart)
            {
                _startDelayTimer = new CooldownTimer();
                _startDelayTimer.Reset();
                _state = SpawnerState.Ready;
            }
        }
        
        private void Update()
        {
#if UNITY_EDITOR
            if (Keyboard.current.nKey.wasPressedThisFrame)
            {
                ForceNextWave();
            }
#endif
            
            switch (_state)
            {
                case SpawnerState.Waiting:
                    UpdateWaitingState();
                    break;

                case SpawnerState.Ready:
                    UpdateReadyState();
                    break;

                case SpawnerState.Spawning:
                    UpdateSpawningState();
                    break;

                case SpawnerState.Refreshing:
                    UpdateRefreshingState();
                    break;
            }
        }
        
        // --------------------------------------------------
        // State Updates
        // --------------------------------------------------
        
        private void UpdateWaitingState()
        {
            if (!useProximityTrigger)
                return;

            if (_proximityTriggered)
                _state = SpawnerState.Ready;
        }
        
        private void UpdateReadyState()
        {
            if (spawnStartDelay <= 0f)
            {
                BeginSpawning();
                return;
            }

            if (_startDelayTimer.Tick(Time.deltaTime))
                BeginSpawning();
        }
        
        private void UpdateSpawningState()
        {
            if (_currentWaveIndex >= spawnData.spawnList.Count)
            {
                OnAllWavesCompleted();
                return;
            }

            if (!_waveFinished)
                UpdateWaveSpawn();
            else
                UpdateWaveCooldown();
        }
        
        private void UpdateRefreshingState()
        {
            if (_refreshTimer.Tick(Time.deltaTime))
            {
                ResetSpawnerInternal();
                _state = SpawnerState.Waiting;
            }
        }
        
        // --------------------------------------------------
        // Spawn Control
        // --------------------------------------------------

        private void BeginSpawning()
        {
            ResetWaveState();
            BuildSpawnSlots();
            BuildSlotOrder();
            _state = SpawnerState.Spawning;
        }

        private void OnAllWavesCompleted()
        {
            if (restartDelay > 0f)
            {
                _refreshTimer = new CooldownTimer();
                _refreshTimer.Reset();
                _state = SpawnerState.Refreshing;
            }
            else
            {
                _state = SpawnerState.Waiting;
            }
            
            OnAllWavesCompletedSignal?.Invoke();
        }

        private void ResetSpawnerInternal()
        {
            _currentWaveIndex = 0;
            ResetWaveState();
        }

        private void ResetWaveState()
        {
            _spawnedElementsInWave = 0;
            _spawnTimer = 0f;
            _waveCooldownTimer = 0f;
            _waveFinished = false;
        }

        // --------------------------------------------------
        // Wave Logic
        // --------------------------------------------------

        private void UpdateWaveSpawn()
        {
            var wave = spawnData.spawnList[_currentWaveIndex];
            int waveSize = wave.entities.Count;

            if (_spawnedElementsInWave >= waveSize)
            {
                _waveFinished = true;
                return;
            }

            _spawnTimer += Time.deltaTime;
            if (_spawnTimer < spawnData.spawnDelay)
                return;

            _spawnTimer = 0f;

            SpawnEntity(
                wave.entities[_spawnedElementsInWave],
                waveSize,
                _spawnedElementsInWave
            );

            _spawnedElementsInWave++;
        }
        
        private void UpdateWaveCooldown()
        {
            _waveCooldownTimer += Time.deltaTime;
            if (_waveCooldownTimer < spawnData.waveDelay)
                return;

            _waveCooldownTimer = 0f;
            _waveFinished = false;
            _spawnedElementsInWave = 0;
            _currentWaveIndex++;

            OnWaveCompletedSignal?.Invoke();
        }

        // --------------------------------------------------
        // Spawn Execution
        // --------------------------------------------------
        
        private void SpawnEntity(CharacterDefinition character, int waveSize, int spawnIndex)
        {
            Vector2 offset = GetSpawnSlotOffset(spawnIndex);

            Vector2 position = (Vector2)transform.position + offset;

            var context = _contextProvider.CreateContext(
                character,
                gameObject,
                _currentWaveIndex,
                spawnIndex
            );

            EntityPoolManager.Instance.Spawn(
                character.prefab,
                position,
                Quaternion.identity,
                context
            );

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
        // Utilities
        // --------------------------------------------------
        
        // Can be built different ways: Lane, Horizontal, 8dir.
        protected abstract void BuildSpawnSlots();

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!useProximityTrigger)
                return;

            if (other.CompareTag("Player"))
                _proximityTriggered = true;
        }

        protected abstract void BuildSlotOrder();
        
        public void ForceNextWave()
        {
            if (_state != SpawnerState.Spawning)
                return;

            _waveFinished = true;
            _waveCooldownTimer = spawnData.waveDelay;
        }
        
        // --------------------------------------------------
        // Internal State Definition
        // --------------------------------------------------
        
        private enum SpawnerState
        {
            Waiting,    // Waiting for trigger or initial state
            Ready,      // Ready to start spawn countdown
            Spawning,   // Currently spawning
            Refreshing  // Waiting for refresh timer
        }
        
#if UNITY_EDITOR
        
        [Header("### Editor Preview ###")]
        [SerializeField, Tooltip("Editor-only: which wave to preview")]
        protected int previewWaveIndex = 0;

        [SerializeField, Tooltip("Editor-only: preview only first N spawns (0 = all)")]
        protected int previewSpawnCount = 0;
        
        protected virtual void OnDrawGizmosSelected()
        {
            if (!spawnData)
                return;
            
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