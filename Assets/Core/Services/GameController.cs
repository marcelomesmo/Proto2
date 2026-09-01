using System;
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
        [Header("Match Stats")]
        [SerializeField] private MatchStatsProvider matchStatsProvider;
        
        public MatchStats MatchStats { get; private set; }
        public MatchRuntime MatchRuntime { get; private set; }
        public UpgradeRuntimeManager UpgradeRuntimeManager { get; private set; }
        public UpgradeManager UpgradeManager { get; private set; }
        
        public event Action OnMatchStarted;
        public event Action OnMatchVictory;
        public event Action OnMatchDefeat;
      
        // --------------------------------------------------
        // Initialization
        // --------------------------------------------------
        
        // Called explicitly by GameBootstrapper
        public void Initialize()
        {
            GameTimeService.Initialize();
            
            MatchRuntime = new MatchRuntime();

            MatchStats = matchStatsProvider != null
                ? matchStatsProvider.CreateStats()
                : new NullMatchStats();

            UpgradeRuntimeManager = new UpgradeRuntimeManager();
            
            UpgradeManager = new UpgradeManager();
            UpgradeManager.Initialize(
                ServiceLocator.Get<ISaveManager>(),
                ServiceLocator.Get<IUpgradeDatabase>(),
                UpgradeRuntimeManager);
        }
        
        // --------------------------------------------------
        // Match Lifecycle
        // --------------------------------------------------
        
        #region Game Flow
        
        public void StartMatch()
        {
            GameTimeService.ResetSpeed();
            PauseService.Resume();
            
            MatchRuntime.BeginMatch();
            MatchStats.OnMatchStart();
            UpgradeRuntimeManager.OnMatchStart();
            
            // Game-layer extension point (no Game types referenced here).
            if (ServiceLocator.TryGet<IMatchLifecycleHandler>(out var hook) && hook != null)
                hook.HandleMatchStart(this);
            
            OnMatchStarted?.Invoke();
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
            if (!CanTickMatch)
                return;

            MatchRuntime.Tick(Time.deltaTime);
            MatchStats.OnMatchTimeUpdated(MatchRuntime.ElapsedTime);
        }
        
        private bool CanTickMatch =>
            MatchRuntime != null &&
            MatchRuntime.IsRunning &&
            !PauseService.IsPaused;
        
        #endregion
        
        // --------------------------------------------------
        // Game End Flow
        // --------------------------------------------------
        
        #region End of Match Flow
        
        public void OnGameDefeat() => StartCoroutine(GameEndRoutine(MatchEndReason.Defeat));
        public void OnGameVictory() => StartCoroutine(GameEndRoutine(MatchEndReason.Victory));
        public void OnGameQuit() => StartCoroutine(GameEndRoutine(MatchEndReason.Quit));
        
        private IEnumerator GameEndRoutine(MatchEndReason reason)
        {
            EndMatch();
            
            if (reason == MatchEndReason.Victory) OnMatchVictory?.Invoke();
            else if (reason == MatchEndReason.Defeat) OnMatchDefeat?.Invoke();
            
            float delay = 0f;

            // IMatchLifecycleHandler will define the coroutine duration. Any Victory effect will be implemente there.
            if (ServiceLocator.TryGet<IMatchLifecycleHandler>(out var hook))
            {
                delay = hook.HandleMatchEndStarted(this, reason);
            }
            
            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);
            
            PauseService.Pause();

            ServiceLocator
                .Get<IMatchLifecycleHandler>()
                ?.HandleMatchEnded(this, reason);
        }
        
        public void ExitMatch()
        {
            Cleanup();
            // Clear when switching worlds/biomes to free asset memory
            ClearPools();
            // TODO: A small optimization we could do, is to only clear when we are actually entering
            //      a new different level, and not at the exit of every level.
            //      This would avoid clearing everytime if the Player is only playing the same level
            //      over and over again.
            SceneLoader.LoadMenu();
        }
        
        private void Cleanup()
        {
            // Release while still paused — systems are frozen
            ServiceLocator.Get<EntityPoolManager>()?.ReleaseAll();
            ShutdownCombatRuntime();
            ServiceLocator.Get<AudioManager>()?.ReleaseAll();
        }
        
        private void ClearPools()
        {
            ServiceLocator.Get<ProjectilePoolManager>()?.Clear();
            ServiceLocator.Get<AreaEffectPoolManager>()?.Clear();
            ServiceLocator.Get<VFXPoolManager>()?.Clear();
            ServiceLocator.Get<EntityPoolManager>()?.Clear();
            
            // TODO: Do we need to clear audio here as well? I don't think so but for future reference check it.
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