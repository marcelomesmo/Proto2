using UnityEngine;

namespace Core.Services
{
    // Scene-local. Owns all music for this specific level.
    // Listens to GameController events and calls AudioManager directly.
    public class MenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject roosterPanel;
        [SerializeField] private GameObject optionsPanel;
        [SerializeField] private GameObject walletPanel;
        
        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;

        private AudioManager _audio;
        
        private void Awake()
        {
            // Later: check save slots here
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
            roosterPanel.SetActive(true);
            walletPanel.SetActive(true);
        }

        public void OnPlayGame()
        {
            if(_audio)
                _audio.Music.Stop();
            
            // TODO: The above stop will result in a brief silence during transition.
            //      This is intentional (remove if not wanted) and we should add a
            //      entering-level Music or entering-level SFX here to add flavor later.
            
            SceneLoader.LoadLevel("Level_01");
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
