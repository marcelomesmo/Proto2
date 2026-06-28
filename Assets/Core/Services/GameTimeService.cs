using UnityEngine;

namespace Core.Services
{
    public static class GameTimeService
    {
        // TODO: If we want it to be designer-facing, we'd expose it through a GameTimeServiceConfig SO.
        private static float[] _speedSteps = { 1f, 2f, 4f };
        private static int _currentSpeedIndex;

        public static float CurrentSpeed => _speedSteps[_currentSpeedIndex];

        public static void Initialize()
        {
            _currentSpeedIndex = 0;
            Apply();
        }

        public static void CycleSpeed()
        {
            if (PauseService.IsPaused) return;

            _currentSpeedIndex = (_currentSpeedIndex + 1) % _speedSteps.Length;
            Apply();
        }

        public static void SetSpeed(float speed)
        {
            // We use this method if we want an external call that directly sets one for the target speed values (defined in _speedSteps).
            
            for (int i = 0; i < _speedSteps.Length; i++)
            {
                if (!Mathf.Approximately(_speedSteps[i], speed)) continue;
                
                _currentSpeedIndex = i;
                
                // Index is updated regardless — Apply is deferred until Resume (case paused).
                if (!PauseService.IsPaused)
                    Apply();
                
                return;
            }

            Debug.LogWarning($"[GameTimeService] Unsupported speed: {speed}");
        }

        public static void ResetSpeed()
        {
            _currentSpeedIndex = 0;
            
            // Index is reset regardless — Apply is deferred until Resume (case paused).
            if (!PauseService.IsPaused)
                Apply();
        }
        
        // For use during scene transitions — applies immediately regardless of pause state,
        // since the pause context is being destroyed along with the scene.
        public static void ForceResetSpeed()
        {
            _currentSpeedIndex = 0;
            Apply();
        }

        public static void Apply()
        {
            float speed = _speedSteps[_currentSpeedIndex];
            Time.timeScale = speed;
            Time.fixedDeltaTime = 0.02f * speed;
        }
        
        public static void Pause()
        {
            Time.timeScale = 0f;
            Time.fixedDeltaTime = 0.02f; // reset to default — no scaling while paused
        }
    }
}
