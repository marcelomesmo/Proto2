using System;
using Core.Gameplay.Entity;
using Core.Util;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Gameplay.Spawner
{
     /*
     * Owns:
     * - Wave progression
     * - Runtime state
     * - Timers
     * - Triggering
     * - Refresh logic
     * - Wave completion flow
     *
     * Executes waves through EntitySpawner.
     */
    [RequireComponent(typeof(EntitySpawner))]
    public sealed class EntitySpawnerRuntimeController : MonoBehaviour
    {
        [Header("Spawn Data")]
        [SerializeField] private EntitySpawnerData spawnData;

        [Header("Trigger Settings")]
        [SerializeField] private bool spawnOnGameStart = true;
        [SerializeField] private float spawnStartDelay;
        [SerializeField] private bool triggerMatchEndOnCompletion = true;
        
        [Header("Proximity Trigger")]
        [SerializeField] private bool useProximityTrigger = false;
        [SerializeField] private float triggerRadius = 5f;
        
        [Header("Refresh Settings")]
        [Tooltip("Time for the spawner to become available again after all waves are completed. 0 = no refresh.")]
        [SerializeField] private float restartDelay = 0f;

        // --------------------------------------------------
        // Internal State
        // --------------------------------------------------

        private SpawnerState _state = SpawnerState.Waiting;

        private CooldownTimer _startDelayTimer;
        private CooldownTimer _refreshTimer;
        private CooldownTimer _spawnTimer;
        private CooldownTimer _waveDurationTimer;
        
        // --------------------------------------------------
        // Runtime
        // --------------------------------------------------

        private EntitySpawner _spawner;

        private int _currentWaveIndex;
        private int _spawnedElementsInWave;

        private bool _spawnFinished;
        private bool _waveExpired;
        private bool _proximityTriggered;
        
        // Enemy tracking
        private int _aliveEnemies;
        
        // --------------------------------------------------
        // Public
        // --------------------------------------------------

        public bool IsSpawning =>
            _state == SpawnerState.Spawning;

        public int CurrentWaveIndex => _currentWaveIndex;

        public int TotalWaveCount =>
            spawnData != null
                ? spawnData.TotalWaveCount
                : 0;
        
        // Human-readable wave number (1-based)
        public int CurrentWaveNumber => CurrentWaveIndex + 1;
        
        public EntitySpawnerData SpawnData => spawnData;
        
        // --------------------------------------------------
        // Events
        // --------------------------------------------------

        public event Action<int> OnWaveStartedSignal;
        public event Action<int> OnWaveCompletedSignal;
        public event Action OnAllWavesCompletedSignal;
        
        // --------------------------------------------------
        // Unity Lifecycle
        // --------------------------------------------------
        
        private void Awake()
        {
            _spawner = GetComponent<EntitySpawner>();
            
            _startDelayTimer = new CooldownTimer();
            _refreshTimer = new CooldownTimer();
            _spawnTimer = new CooldownTimer();
            _waveDurationTimer = new CooldownTimer();
        }
        
        private void Start()
        {
            ResetSpawnerInternal();

            if (spawnOnGameStart)
            {
                _startDelayTimer.Start(spawnStartDelay);
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
            {
                _startDelayTimer.Start(spawnStartDelay);
                _state = SpawnerState.Ready;
            }
        }
        
        private void UpdateReadyState()
        {
            if (spawnStartDelay <= 0f)
            {
                BeginSpawning();
                return;
            }

            if (_startDelayTimer.Tick(Time.deltaTime))
            {
                BeginSpawning();
            }
        }
        
        private void UpdateSpawningState()
        {
            if (spawnData == null)
            {
                Debug.LogError("[EntitySpawnerRuntimeController] SpawnData is null");
                return;
            }

            if (_currentWaveIndex >= spawnData.TotalWaveCount)
            {
                OnAllWavesCompleted();
                return;
            }

            UpdateWaveSpawning();

            UpdateWaveDuration();

            EvaluateWaveCompletion();
        }
        
        private void UpdateRefreshingState()
        {
            if (!_refreshTimer.Tick(Time.deltaTime))
                return;

            ResetSpawnerInternal();

            _state = SpawnerState.Waiting;

            if (spawnOnGameStart)
            {
                _startDelayTimer.Start(spawnStartDelay);
                _state = SpawnerState.Ready;
            }
        }
        
        // --------------------------------------------------
        // Spawn Control
        // --------------------------------------------------

        private void BeginSpawning()
        {
            StartWave(_currentWaveIndex);
        }

        private void StartWave(int waveIndex)
        {
            ResetWaveRuntime();

            _spawner.PrepareWave();
            
            var wave = spawnData.waves[waveIndex];

            // Start spawning timer
            _spawnTimer.Start(spawnData.spawnDelay);
            
            // Start wave duration timer ONLY if allowed
            if (!wave.killAllRequired)
            {
                float duration =
                    wave.overrideWaveDuration >= 0f
                        ? wave.overrideWaveDuration
                        : spawnData.defaultWaveDuration;

                _waveDurationTimer.Start(duration);
            }

            _state = SpawnerState.Spawning;

            OnWaveStartedSignal?.Invoke(_currentWaveIndex);
        }

        // --------------------------------------------------
        // Wave Logic
        // --------------------------------------------------

        private void UpdateWaveSpawning()
        {
            if (_spawnFinished)
                return;
            
            var wave =
                spawnData.waves[_currentWaveIndex];

            int waveSize =
                wave.entities.Count;

            if (_spawnedElementsInWave >= waveSize)
            {
                _spawnFinished = true;
                return;
            }

            if (!_spawnTimer.Tick(Time.deltaTime))
                return;

            _spawner.SpawnWaveEntity(
                wave,
                _currentWaveIndex,
                _spawnedElementsInWave
            );

            _spawnedElementsInWave++;
            
            _spawnTimer.Start(spawnData.spawnDelay);
        }

        private void UpdateWaveDuration()
        {
            var wave =
                spawnData.waves[_currentWaveIndex];

            if (wave.killAllRequired)
                return;

            if (_waveExpired)
                return;

            if (_waveDurationTimer.Tick(Time.deltaTime))
            {
                _waveExpired = true;
            }
        }
        
        private void EvaluateWaveCompletion()
        {
            // Prevent instant completion
            if (!_spawnFinished)
                return;

            var wave =
                spawnData.waves[_currentWaveIndex];
            
            bool isFinalWave =
                _currentWaveIndex >= spawnData.TotalWaveCount - 1;
            
            // Final wave of a match-ending spawner
            if (isFinalWave && triggerMatchEndOnCompletion)
            {
                if (_aliveEnemies <= 0)
                {
                    CompleteCurrentWave();
                }

                return;
            }

            // Mandatory kill-all wave
            if (wave.killAllRequired)
            {
                if (_aliveEnemies <= 0)
                {
                    CompleteCurrentWave();
                }

                return;
            }

            // Standard wave:
            // complete on:
            // - timer expiration
            // OR
            // - all enemies dead
            if (_waveExpired || _aliveEnemies <= 0)
            {
                CompleteCurrentWave();
            }
        }

        private void CompleteCurrentWave()
        {
            OnWaveCompletedSignal?.Invoke(_currentWaveIndex);

            _currentWaveIndex++;

            if (_currentWaveIndex >= spawnData.TotalWaveCount)
            {
                OnAllWavesCompleted();
                return;
            }
            
            StartWave(_currentWaveIndex);
        }
        
        private void ResetSpawnerInternal()
        {
            _currentWaveIndex = 0;
            
            _aliveEnemies = 0;
            
            ResetWaveRuntime();
        }
        
        private void ResetWaveRuntime()
        {
            _spawnedElementsInWave = 0;
            
            _spawnFinished = false;
            _waveExpired = false;
        }
        
        private void OnAllWavesCompleted()
        {
            _state = SpawnerState.Waiting;
            
            OnAllWavesCompletedSignal?.Invoke();

            if (triggerMatchEndOnCompletion)
                return;
            
            if (restartDelay > 0f)
            {
                _refreshTimer.Start(restartDelay);
                _state = SpawnerState.Refreshing;
            }
        }
        
        // --------------------------------------------------
        // Enemy Tracking
        // --------------------------------------------------

        public void RegisterSpawnedEnemy(EntityController entity)
        {
            if (entity == null)
                return;

            _aliveEnemies++;
            
            entity.DeathSignal -= OnSpawnedEnemyDeath;
            entity.DeathSignal += OnSpawnedEnemyDeath;
        }
        
        private void OnSpawnedEnemyDeath(EntityController entity)
        {
            if (entity == null)
                return;

            entity.DeathSignal -= OnSpawnedEnemyDeath;

            RegisterEnemyKilled();
        }
        
        public void RegisterEnemyKilled()
        {
            _aliveEnemies--;
            
            if (_aliveEnemies < 0)
                _aliveEnemies = 0;
        }
        
        // --------------------------------------------------
        // Utilities
        // --------------------------------------------------

        public void ForceNextWave()
        {
            if (_state != SpawnerState.Spawning)
                return;
            
            CompleteCurrentWave();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!useProximityTrigger)
                return;

            if (other.CompareTag("Player"))
            {
                _proximityTriggered = true;
            }
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

        private void OnDrawGizmosSelected()
        {
            if (!useProximityTrigger)
                return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(
                transform.position,
                triggerRadius
            );
        }

#endif
    }
}