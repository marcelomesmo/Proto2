using UnityEngine;

namespace Core.Services
{
    public static class PauseService
    {
        public static bool IsPaused { get; private set; }

        public static void Pause()
        {
            if (IsPaused) return;
            
            IsPaused = true;
            GameTimeService.Pause();
            
            InputMapController.DisableGameplay();
            // Only for games that hide cursor during gameplay.
            //Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.None;
        }

        public static void Resume()
        {
            if (!IsPaused)
            {
                Debug.LogWarning("[PauseService] Trying to Resume game with game already running.");
                return;
            }
            
            IsPaused = false;
            GameTimeService.Apply();
            
            InputMapController.EnableGameplay();
            // Only for games that hide cursor during gameplay.
            //Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Locked;
        }
    }
}