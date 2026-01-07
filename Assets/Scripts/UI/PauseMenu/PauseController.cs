using Enum;
using Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UI.PauseMenu
{
    public class PauseController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject pauseRoot;
        [SerializeField] private GameObject optionsRoot;
        
        [Header("UX")]
        [SerializeField] private GameObject defaultPauseSelection;
        [SerializeField] private GameObject defaultOptionsSelection;
        
        [Header("Gameplay HUD")]
        // A list of canvas objects which are used during gameplay (when the main ui is turned off)
        public Canvas[] gameplayCanvases;
        
        private PauseState _state = PauseState.Playing;
        
        private InputAction m_MenuAction;
        
        private void Awake()
        {
            pauseRoot.SetActive(false);
            optionsRoot.SetActive(false);
            
            m_MenuAction = InputSystem.actions.FindAction("UI/Cancel");
        }
        
        private void Update()
        {
            if (m_MenuAction.WasPressedThisFrame())
            {
                HandleEscape();
            }
        }
        
        private void HandleEscape()
        {
            switch (_state)
            {
                case PauseState.Playing:
                    Pause();
                    break;

                case PauseState.Paused:
                    Resume();
                    break;

                case PauseState.Options:
                    CloseOptions();
                    break;
            }
        }
        
        public void Pause()
        {
            _state = PauseState.Paused;
            
            PauseService.Pause();

            pauseRoot.SetActive(true);
            optionsRoot.SetActive(false);
            foreach (var i in gameplayCanvases) i.gameObject.SetActive(false);
            
            // Set default selection
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(defaultPauseSelection);
        }

        public void Resume()
        {
            _state = PauseState.Playing;
            
            PauseService.Resume();

            pauseRoot.SetActive(false);
            optionsRoot.SetActive(false);
            foreach (var i in gameplayCanvases) i.gameObject.SetActive(true);
            
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void OpenOptions()
        {
            _state = PauseState.Options;
            
            pauseRoot.SetActive(false);
            optionsRoot.SetActive(true);
            
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(defaultOptionsSelection);
        }
        
        public void CloseOptions()
        {
            _state = PauseState.Paused;

            optionsRoot.SetActive(false);
            pauseRoot.SetActive(true);
        }

        public void QuitToMenu()
        {
            PauseService.Resume(); // restores input + time
            SceneLoader.LoadMenu();
        }
    }
}
