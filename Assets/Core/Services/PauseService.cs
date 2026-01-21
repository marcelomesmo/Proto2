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
            
            ServiceLocator.Get<GameController>().PauseGame();
            InputMapController.DisableGameplay();
            
            //Cursor.visible = true;
            //Cursor.lockState = CursorLockMode.None;
        }

        public static void Resume()
        {
            if (!IsPaused) return;
            
            IsPaused = false;
            
            ServiceLocator.Get<GameController>().ResumeGame();
            InputMapController.EnableGameplay();
            
            // Only for games that hide cursor during gameplay.
            //Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Locked;
        }
    }
}