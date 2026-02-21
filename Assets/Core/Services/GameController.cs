using System.Collections.Generic;
using Core.Services.Manager;
using Core.Services.Meta;
using Core.Services.Save;
using Core.Upgrades;
using Core.Upgrades.Database;
using UnityEngine;

namespace Core.Services
{
    public class GameController : MonoBehaviour
    {
        [Header("Game Speed")]
        [SerializeField] private float[] speedSteps = { 1f, 2f, 4f };
        
        [Header("Match Stats")]
        [SerializeField] private MatchStatsProvider matchStatsProvider;
        
        private int _currentSpeedIndex;
        private bool _isPaused;
        
        public float CurrentGameSpeed => speedSteps[_currentSpeedIndex];
        public bool IsPaused => _isPaused;
        
        public MatchStats MatchStats { get; private set; }
        public MatchRuntime MatchRuntime { get; private set; }
        public UpgradeManager UpgradeManager { get; private set; }
      
        // --------------------------------------------------
        // Initialization
        // --------------------------------------------------
        
        // Called explicitly by GameBootstrapper
        public void Initialize()
        {
            MatchRuntime = new MatchRuntime();

            MatchStats = matchStatsProvider != null
                ? matchStatsProvider.CreateStats()
                : new NullMatchStats();

            UpgradeManager = new UpgradeManager();
            
            SetGameSpeed(1f);
        }
        
        // --------------------------------------------------
        // Match Lifecycle
        // --------------------------------------------------
        
        #region Game Flow
        
        public void StartMatch()
        {
            _isPaused = false;
            MatchRuntime.BeginMatch();
            MatchStats.OnMatchStart();
            UpgradeManager.OnMatchStart();
            LoadRuntimeUpgrades();
        }
        
        private void EndMatch()
        {
            MatchRuntime.EndMatch();
            MatchStats.OnMatchEnd();
            UpgradeManager.OnMatchEnd();
            ServiceLocator.Get<ISaveManager>().OnMatchEnd();
        }
        
        private void Update()
        {
            if (_isPaused)
                return;
            
            // Update Game Controller

            if (MatchRuntime == null)
                return;
            
            /*
             * Above or:
                 bool CanTickMatch =>
                 MatchRuntime != null &&
                 !_isPaused &&
                 MatchRuntime.IsRunning;
             */

            MatchRuntime.Tick(Time.deltaTime);
            
            MatchStats.OnMatchTimeUpdated(MatchRuntime.ElapsedTime);
        }
        
        private bool CanTickMatch =>
            !_isPaused && MatchRuntime.IsRunning;
        
        #endregion
        
        // --------------------------------------------------
        // Game Speed
        // --------------------------------------------------
        
        #region Game Speed

        public void CycleGameSpeed()
        {
            if (_isPaused)
                return;
            
            _currentSpeedIndex = (_currentSpeedIndex + 1) % speedSteps.Length;
            ApplyGameSpeed();
        }

        public void SetGameSpeed(float speed)
        {
            for (int i = 0; i < speedSteps.Length; i++)
            {
                if (Mathf.Approximately(speedSteps[i], speed))
                {
                    _currentSpeedIndex = i;
                    ApplyGameSpeed();
                    return;
                }
            }

            Debug.LogWarning($"[GameController] Unsupported game speed: {speed}");
        }

        private void ApplyGameSpeed()
        {
            float speed = speedSteps[_currentSpeedIndex];

            Time.timeScale = speed;
            Time.fixedDeltaTime = 0.02f * speed;

            //Debug.Log($"[GameController] Game speed set to {speed}x");
        }

        #endregion
        
        // --------------------------------------------------
        // Pause Control
        // --------------------------------------------------
        
        #region Pause Control

        public void PauseGame()
        {
            if (_isPaused)
                return;

            _isPaused = true;
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            if (!_isPaused)
                return;

            _isPaused = false;
            ApplyGameSpeed();
        }
        
        #endregion
        
        // --------------------------------------------------
        // Game End Flow
        // --------------------------------------------------
        
        #region End of Match Flow
        
        public void OnGameDefeat()
        {
            // play defeat UI, analytics, etc.
            
            EndMatch();
            
            Cleanup();
            SceneLoader.LoadMenu();
        }
        
        public void OnGameVictory()
        {
            // play victory UI, cinematics, etc.
            
            EndMatch();
            
            Cleanup();
            SceneLoader.LoadMenu();
        }
        
        private void Cleanup()
        {
            SetGameSpeed(1f);
            _isPaused = false;

            ServiceLocator.Get<EntityPoolManager>()?.ReleaseAll();
            ServiceLocator.Get<ProjectilePoolManager>()?.ReleaseAll();
            ServiceLocator.Get<VFXPoolManager>()?.ReleaseAll();
            ServiceLocator.Get<AudioManager>()?.ReleaseAll();
        }
        
        #endregion
        
        // --------------------------------------------------
        // Game Save
        // --------------------------------------------------
        
        #region Game Save
        
        
        
        #endregion
        
        // --------------------------------------------------
        // Game Load
        // --------------------------------------------------
        
        #region Game Load
        
        private void LoadRuntimeUpgrades()
        {
            var save =
                ServiceLocator.Get<ISaveManager>() as Game.Services.Save.GameSaveManager;

            if (save == null)
                return;

            if (!ServiceLocator.TryGet<UpgradeDatabase>(out var db) || db == null)
            {
                Debug.LogError("[GameController] UpgradeDatabase service not registered.");
                return;
            }

            var definitions = new List<UpgradeDefinition>();

            // Only purchased upgrades apply to match.
            foreach (var p in save.Profile.upgrades)
            {
                if (!p.unlocked)
                    continue;

                // Only apply upgrades that actually have purchased levels.
                if (p.level <= 0)
                    continue;
                
                if (db.TryGet(p.id, out var def))
                    definitions.Add(def);
            }

            UpgradeManager.LoadUpgrades(definitions);
        }
        
        #endregion
    }
}
