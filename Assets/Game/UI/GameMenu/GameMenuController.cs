using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Game.UI.GameMenu
{
    public sealed class GameMenuController : MonoBehaviour
    {
        private enum MenuTab
        {
            Characters,
            Upgrades,
            Settings
        }
        
        [Header("Menu")]
        [SerializeField] private GameObject menuRoot;

        [Header("Content Tab")]
        [SerializeField] private GameObject charactersRoot;
        [SerializeField] private GameObject upgradesRoot;
        [SerializeField] private GameObject settingsRoot;

        [Header("New Tab Opener")]
        [SerializeField] private GameObject upgradesPopupRoot;
        
        [Header("Default Selection")]
        [SerializeField] private GameObject defaultCharactersSelection;
        [SerializeField] private GameObject defaultUpgradesSelection;
        [SerializeField] private GameObject defaultOptionsSelection;
        
        private InputAction _menuAction;

        private MenuTab _currentTab = MenuTab.Characters;

        private bool _isOpen;

        // --------------------------------------------------
        // Unity Lifecycle
        // --------------------------------------------------

        private void Awake()
        {
            menuRoot.SetActive(false);

            charactersRoot.SetActive(false);
            upgradesRoot.SetActive(false);
            settingsRoot.SetActive(false);
            
            upgradesPopupRoot.SetActive(false);

            _menuAction = InputSystem.actions.FindAction("UI/Cancel");
        }
        
        // --------------------------------------------------
        // Menu
        // --------------------------------------------------

        public void ToggleMenu()
        {
            if (_isOpen)
                CloseMenu();
            else
                OpenMenu();
        }

        public void OpenMenu()
        {
            if (_isOpen)
                return;

            _isOpen = true;

            menuRoot.SetActive(true);

            ShowCurrentTab();
        }

        public void CloseMenu()
        {
            if (!_isOpen)
                return;

            _isOpen = false;

            menuRoot.SetActive(false);

            EventSystem.current?.SetSelectedGameObject(null);
        }
        
        // --------------------------------------------------
        // Tabs
        // --------------------------------------------------

        public void OpenCharacters()
        {
            SetTab(MenuTab.Characters);
        }

        public void OpenUpgrades()
        {
            SetTab(MenuTab.Upgrades);
        }

        public void OpenSettings()
        {
            SetTab(MenuTab.Settings);
        }

        private void SetTab(MenuTab tab)
        {
            _currentTab = tab;

            if (!_isOpen)
                OpenMenu();
            else
                ShowCurrentTab();
        }

        private void ShowCurrentTab()
        {
            charactersRoot.SetActive(_currentTab == MenuTab.Characters);
            upgradesRoot.SetActive(_currentTab == MenuTab.Upgrades);
            settingsRoot.SetActive(_currentTab == MenuTab.Settings);

            GameObject defaultSelection = GetCurrentDefaultSelection();

            EventSystem.current?.SetSelectedGameObject(null);

            if (defaultSelection != null)
            {
                EventSystem.current?.SetSelectedGameObject(defaultSelection);
            }
        }
        
        private GameObject GetCurrentDefaultSelection()
        {
            return _currentTab switch
            {
                MenuTab.Characters => defaultCharactersSelection,
                MenuTab.Upgrades => defaultUpgradesSelection,
                MenuTab.Settings => defaultOptionsSelection,
                _ => null
            };
        }
        
        // --------------------------------------------------
        // Pop-ups
        // --------------------------------------------------
        
        public void ToggleUpgradesPopup()
        {
            bool currentState = upgradesPopupRoot.activeSelf;
            
            upgradesPopupRoot.SetActive(!currentState);
        }

        // --------------------------------------------------
        // Application
        // --------------------------------------------------

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
