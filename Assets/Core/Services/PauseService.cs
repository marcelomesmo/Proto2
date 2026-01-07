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
            Time.timeScale = 0f;
            
            InputMapController.DisableGameplay();
            //Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.None;
        }

        public static void Resume()
        {
            if (!IsPaused) return;
            
            IsPaused = false;
            Time.timeScale = 1f;
            
            InputMapController.EnableGameplay();
            // Only for games that hide cursor during gameplay.
            //Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Locked;
        }
    }
}