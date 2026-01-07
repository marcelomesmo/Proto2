using UnityEngine.InputSystem;

namespace Core.Services
{
    public static class InputMapController
    {
        private static InputActionAsset _actions;
        
        private static InputActionMap _playerMap;
        private static InputActionMap _uiMap;

        private static bool _initialized;

        public static void Initialize(InputActionAsset asset)
        {
            if (_initialized)
                return;
            
            _actions = asset;
            
            _playerMap = _actions.FindActionMap("Player", true);
            _uiMap = _actions.FindActionMap("UI", true);
            
            // UI is ALWAYS enabled
            _uiMap.Enable();

            // Default state: gameplay active
            _playerMap.Enable();

            _initialized = true;
        }

        // -------------------------
        // GAMEPLAY CONTROL
        // -------------------------

        public static void EnableGameplay()
        {
            if (!_initialized) return;

            if (!_playerMap.enabled)
                _playerMap.Enable();
        }

        public static void DisableGameplay()
        {
            if (!_initialized) return;

            if (_playerMap.enabled)
                _playerMap.Disable();
        }

        // -------------------------
        // SAFETY / DEBUG
        // -------------------------

        public static bool IsGameplayEnabled =>
            _initialized && _playerMap.enabled;

        public static bool IsUIEnabled =>
            _initialized && _uiMap.enabled;
    }
}
