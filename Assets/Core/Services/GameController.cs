using Core.Services.Manager;
using Core.Services.Meta;
using UnityEngine;

namespace Core.Services
{
    public class GameController : MonoBehaviour
    {
        [Header("Game Speed")]
        [SerializeField] private float[] speedSteps = { 1f, 2f, 4f };
        
        private int _currentSpeedIndex;
        private bool _isPaused;
        public float CurrentGameSpeed => speedSteps[_currentSpeedIndex];
        public bool IsPaused => _isPaused;
      
        // -----------------------------
        // Match
        // -----------------------------
        
        public MatchStats MatchStats { get; private set; }

        // Called explicitly by GameBootstrapper
        public void Initialize()
        {
            MatchStats = new MatchStats();
            
            SetGameSpeed(1f);
            _isPaused = false;
        }
        
        private void Update()
        {
            // Match time automatically respects pause & game speed
            MatchStats?.Tick(Time.deltaTime);
        }
        
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

            Debug.Log($"[GameController] Game speed set to {speed}x");
        }

        #endregion
        
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
        
        #region Game End Flow
        
        public void StartMatch()
        {
            MatchStats?.StartMatch();
        }
        
        public void OnGameDefeat()
        {
            // play defeat UI, analytics, etc.
            
            EndMatch();
            
            SceneLoader.LoadMenu();
        }
        
        public void OnGameVictory()
        {
            // play victory UI, cinematics, etc.
            
            EndMatch();
            
            SceneLoader.LoadMenu();
        }
        
        private void EndMatch()
        {
            MatchStats?.StopMatch();

            SetGameSpeed(1f);
            _isPaused = false;
            
            ServiceLocator.Get<EntityPoolManager>().ReleaseAll();
            ServiceLocator.Get<ProjectilePoolManager>().ReleaseAll();
            ServiceLocator.Get<VFXPoolManager>().ReleaseAll();
            ServiceLocator.Get<AudioManager>().ReleaseAll();
        }
        
        #endregion
    }
}
