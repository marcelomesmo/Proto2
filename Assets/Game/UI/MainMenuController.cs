using Core.Services;
using Game.UI.Roster;
using UnityEngine;

namespace Game.UI
{
    // Scene-local. Owns all music for the main menu.
    // Listens to GameController events and calls AudioManager directly.
    public class MainMenuController : MonoBehaviour
    {
        [Header("Main Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject rosterPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject walletPanel;
        
        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;

        private AudioManager _audio;
        
        private void Awake()
        {
            // Later: check save slots here?
            ShowMain();
        }

        public void Start()
        {
            _audio = ServiceLocator.Get<AudioManager>();
            
            if(_audio)
                _audio.Music.Play(menuMusic);
        }

        // ---------- MAIN MENU ----------

        public void OnStartGame()
        {
            // TODO: Might remove this later and just open the rooster straight ahead.
            
            mainPanel.SetActive(false);
            rosterPanel.SetActive(true);
            walletPanel.SetActive(true);
        }

        public void OnPlayGame()
        {
            if(_audio)
                _audio.Music.Stop();
            // TODO: The above stop will result in a brief silence during transition.
            //      This is intentional (remove if not wanted) and we should add a
            //      entering-level Music or entering-level SFX here to add flavor later.

            var rosterController = rosterPanel.GetComponentInChildren<RosterPanelController>();
            if (!rosterController)
            {
                Debug.LogError("[MainMenuController] Missing RosterPanelController in rosterPanel");
                return;
            }

            if (!rosterController.HasCharacterSelected)
            {
                // TODO: play vfx or sfx feedback here when there's no character in party.
                return;
            }

            if (rosterController.IsCurrentLevelLocked)
            {
                // TODO: play vfx or sfx feedback here when there's not a valid level.
                return;
            }
            
            SceneLoader.LoadLevel(rosterController.GetLevelSelectedSceneName);
        }

        public void OnOptions()
        {
            mainPanel.SetActive(false);
            optionsPanel.SetActive(true);
        }

        public void OnExit()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        // ---------- OPTIONS ----------

        public void OnOptionsBack()
        {
            ShowMain();
        }

        private void ShowMain()
        {
            mainPanel.SetActive(true);
            optionsPanel.SetActive(false);
        }
    }
}
