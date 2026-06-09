using System.Collections;
using Core.Enum;
using Core.Services.Manager;
using Core.Services.Meta;
using Core.Services.Save;
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
        public UpgradeRuntimeManager UpgradeRuntimeManager { get; private set; }
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

            UpgradeRuntimeManager = new UpgradeRuntimeManager();
            
            UpgradeManager = new UpgradeManager();
            UpgradeManager.Initialize(
                ServiceLocator.Get<ISaveManager>(),
                ServiceLocator.Get<IUpgradeDatabase>());
            
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
            UpgradeRuntimeManager.OnMatchStart();
            
            // Game-layer extension point (no Game types referenced here).
            if (ServiceLocator.TryGet<IMatchLifecycleHandler>(out var hook) && hook != null)
                hook.HandleMatchStart(this);
        }
        
        private void EndMatch()
        {
            MatchRuntime.EndMatch();
            
            ShutdownCombatRuntime();
            
            MatchStats.OnMatchEnd();
            UpgradeRuntimeManager.OnMatchEnd();
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
            StartCoroutine(GameEndRoutine(MatchEndReason.Defeat));
        }
        
        public void OnGameVictory()
        {
            StartCoroutine(GameEndRoutine(MatchEndReason.Victory));
        }
        
        public void OnGameQuit()
        {
            StartCoroutine(GameEndRoutine(MatchEndReason.Quit));
        }
        
        private IEnumerator GameEndRoutine(MatchEndReason reason)
        {
            EndMatch();
            
            float delay = 0f;

            // IMatchLifecycleHandler will define the coroutine duration. Any Victory effect will be implemente there.
            if (ServiceLocator.TryGet<IMatchLifecycleHandler>(out var hook))
            {
                delay = hook.HandleMatchEndStarted(this, reason);
            }
            
            if (delay > 0f)
                yield return new WaitForSeconds(delay);
            
            PauseGame();

            ServiceLocator
                .Get<IMatchLifecycleHandler>()
                ?.HandleMatchEnded(this, reason);
        }
        
        public void ExitMatch()
        {
            Cleanup();
            
            // TODO:
            // Later, when introducing:
            // - multiple Worlds/biomes
            // - Addressables
            // - large world-specific asset sets
            //
            // we should also CLEAR cached pools here
            // (not only Release active instances).
            //
            // ReleaseAll():
            // - despawns active runtime objects
            // - keeps inactive cached instances alive for reuse
            //
            // Clear():
            // - destroys cached pooled instances
            // - releases asset references
            // - allows Addressables/world assets to unload correctly
            //
            // Otherwise pooled inactive objects may keep old World
            // assets resident in memory across matches/transitions.
            //
            // ProjectilePoolManager.Clear();
            // EntityPoolManager.Clear();
            // VFXPoolManager.Clear();
            
            SceneLoader.LoadMenu();
        }
        
        private void Cleanup()
        {
            SetGameSpeed(1f);
            _isPaused = false;

            ServiceLocator.Get<EntityPoolManager>()?.ReleaseAll();
            ShutdownCombatRuntime();
            ServiceLocator.Get<AudioManager>()?.ReleaseAll();
        }
        
        private void ShutdownCombatRuntime()
        {
            ServiceLocator.Get<ProjectilePoolManager>()?.ReleaseAll();
            ServiceLocator.Get<AreaEffectPoolManager>()?.ReleaseAll();
            ServiceLocator.Get<VFXPoolManager>()?.ReleaseAll();
        }
        
        #endregion
    }
}