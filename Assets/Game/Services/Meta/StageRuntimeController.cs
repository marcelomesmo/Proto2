using System;
using System.Collections.Generic;
using Core.Gameplay.Spawner;
using Game.Services.Save;
using UnityEngine;

namespace Game.Services.Meta
{
    //
    // Owns runtime Stage / Level progression.
    //
    // Hierarchy:
    //
    // Stage
    //   -> Level
    //       -> EntitySpawnerRuntimeController
    //           -> Waves
    //
    // Stage and Level identity comes directly from list position.
    //
    // Responsibilities:
    // - Resolve saved Stage / Level position.
    // - Start/stop the current Level spawner.
    // - Move forward/backward through Levels and Stages.
    // - Persist the current Stage / Level position.
    //
    // Does NOT:
    // - End/restart matches.
    // - Spawn/despawn player entities.
    // - Release pools.
    // - Decide what Defeat/Victory means.
    //
    public sealed class StageRuntimeController : MonoBehaviour
    {
        [Serializable]
        private sealed class StageDefinition
        {
            [SerializeField] private List<EntitySpawnerData> levels = new();

            public IReadOnlyList<EntitySpawnerData> Levels => levels;
        }
        
        [Header("Spawner")]
        [SerializeField] private EntitySpawnerRuntimeController spawner;
        
        [Header("Stages")]
        [SerializeField] private List<StageDefinition> stages = new();

        private GameSaveManager _saveManager;

        private int _currentStageIndex;
        private int _currentLevelIndex;

        private bool _initialized;
        
        // --------------------------------------------------
        // Public State
        // --------------------------------------------------

        public int CurrentStageIndex => _currentStageIndex;
        public int CurrentLevelIndex => _currentLevelIndex;

        // UI-friendly one-based values.
        public int CurrentStageNumber => _currentStageIndex + 1;
        public int CurrentLevelNumber => _currentLevelIndex + 1;

        public int CurrentStageLevelCount => CurrentStage.Levels.Count;
        
        public event Action<int, int> OnLevelStarted;
        public event Action<int, int> OnLevelCompleted;
        public event Action<int, int> OnProgressionChanged;
        
        // --------------------------------------------------
        // Internal access
        // --------------------------------------------------

        private StageDefinition CurrentStage => stages[_currentStageIndex];
        private EntitySpawnerData CurrentLevelData => CurrentStage.Levels[_currentLevelIndex];
        
        // --------------------------------------------------
        // Initialization
        // --------------------------------------------------

        public bool Initialize(GameSaveManager saveManager)
        {
            if (_initialized)
                return true;

            if (saveManager == null)
            {
                Debug.LogError("[StageRuntimeController] SaveManager is null.", this);
                return false;
            }

            if (!ValidateConfiguration())
                return false;

            _saveManager = saveManager;
            
            _currentStageIndex = _saveManager.Profile.currentStageIndex;
            _currentLevelIndex = _saveManager.Profile.currentLevelIndex;
            
            if (!IsValidPosition(_currentStageIndex, _currentLevelIndex))
            {
                Debug.LogWarning("[StageRuntimeController] Saved progression position is invalid. Resetting to Stage 1-1.", this);
                
                // New save or invalid development save:
                // start from the first configured Level.
                _currentStageIndex = 0;
                _currentLevelIndex = 0;

                SaveCurrentPosition();
            }

            BindSpawner();
            
            _initialized = true;

            return true;
        }

        // --------------------------------------------------
        // Level Runtime
        // --------------------------------------------------

        public void StartCurrentLevel()
        {
            EnsureInitialized();

            // The shared spawner must not retain runtime activity from the previous Level.
            spawner.StopSpawner();

            if (!spawner.TrySetSpawnData(CurrentLevelData))
            {
                Debug.LogError($"[StageRuntimeController] Could not configure Stage {CurrentStageNumber}, Level {CurrentLevelNumber}.", this);
                return;
            }

            spawner.StartSpawner();

            OnLevelStarted?.Invoke(_currentStageIndex, _currentLevelIndex);
        }

        public void StopCurrentLevel()
        {
            if (!_initialized)
                return;
            
            spawner.StopSpawner();
        }

        private void HandleSpawnerCompleted()
        {
            // Capture the completed position before notifying external
            // systems. MatchSceneController may advance progression
            // in response to this event.
            int completedStageIndex = _currentStageIndex;
            int completedLevelIndex = _currentLevelIndex;

            OnLevelCompleted?.Invoke(completedStageIndex, completedLevelIndex);
        }

        // --------------------------------------------------
        // Spawner Binding
        // --------------------------------------------------

        private void BindSpawner()
        {
            spawner.OnAllWavesCompletedSignal -= HandleSpawnerCompleted;
            spawner.OnAllWavesCompletedSignal += HandleSpawnerCompleted;
        }

        private void UnbindSpawner()
        {
            if (spawner == null)
                return;

            spawner.OnAllWavesCompletedSignal -= HandleSpawnerCompleted;
        }

        // --------------------------------------------------
        // Progression
        // --------------------------------------------------

        public bool TryMoveToNextLevel()
        {
            EnsureInitialized();

            int lastLevelIndex = CurrentStage.Levels.Count - 1;

            // Another Level exists inside this Stage.
            if (_currentLevelIndex < lastLevelIndex)
            {
                _currentLevelIndex++;

                SaveCurrentPosition();
                
                return true;
            }

            // No more Levels in this Stage.
            // Move to Level 1 of the next Stage.
            if (_currentStageIndex < stages.Count - 1)
            {
                _currentStageIndex++;
                _currentLevelIndex = 0;

                SaveCurrentPosition();
                
                return true;
            }

            // Final Level of final Stage.
            // Do not change the saved position.
            // MatchSceneController interprets false as true game victory.
            return false;
        }

        public void ApplyDefeatFallback()
        {
            EnsureInitialized();

            // Stage boundaries are checkpoints.
            // Level 1 of a Stage never falls back into the previous Stage.
            if (_currentLevelIndex <= 0)
                return;
            
            _currentLevelIndex--;
            
            SaveCurrentPosition();
        }

        // --------------------------------------------------
        // Persistence
        // --------------------------------------------------

        private void SaveCurrentPosition()
        {
            _saveManager.Profile.currentStageIndex = _currentStageIndex;
            _saveManager.Profile.currentLevelIndex = _currentLevelIndex;

            _saveManager.Save();

            OnProgressionChanged?.Invoke(
                _currentStageIndex,
                _currentLevelIndex);
        }

        // --------------------------------------------------
        // Validation
        // --------------------------------------------------

        private bool ValidateConfiguration()
        {
            if (spawner == null)
            {
                Debug.LogError("[StageRuntimeController] Missing EntitySpawnerRuntimeController.", this);
                return false;
            }

            if (stages == null || stages.Count == 0)
            {
                Debug.LogError("[StageRuntimeController] No stages configured.", this);
                return false;
            }

            for (int stageIndex = 0; stageIndex < stages.Count; stageIndex++)
            {
                StageDefinition stage = stages[stageIndex];

                if (stage == null ||
                    stage.Levels == null ||
                    stage.Levels.Count == 0)
                {
                    Debug.LogError($"[StageRuntimeController] Stage {stageIndex + 1} has no Levels.", this);
                    return false;
                }

                for (int levelIndex = 0; levelIndex < stage.Levels.Count; levelIndex++)
                {
                    if (stage.Levels[levelIndex] != null)
                        continue;

                    Debug.LogError($"[StageRuntimeController] Stage {stageIndex + 1}, Level {levelIndex + 1} has no Spawner.", this);

                    return false;
                }
            }

            return true;
        }
        
        private bool IsValidPosition(int stageIndex, int levelIndex)
        {
            if (stageIndex < 0 || stageIndex >= stages.Count)
                return false;

            StageDefinition stage = stages[stageIndex];

            if (stage == null || stage.Levels == null)
                return false;

            return levelIndex >= 0 &&
                   levelIndex < stage.Levels.Count;
        }
        
        private void EnsureInitialized()
        {
            if (!_initialized)
            {
                throw new InvalidOperationException("[StageRuntimeController] Controller has not been initialized.");
            }
        }

        // --------------------------------------------------
        // Unity Lifecycle
        // --------------------------------------------------
        
        private void OnDestroy()
        {
            UnbindSpawner();
        }
    }
}