using UnityEngine;

namespace Core.Services
{
    public class MenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject roosterPanel;
        [SerializeField] private GameObject optionsPanel;

        private void Awake()
        {
            // Later: check save slots here
            ShowMain();
        }

        // ---------- MAIN MENU ----------

        public void OnStartGame()
        {
            // TODO: Might remove this later and just open the rooster straight ahead.
            
            mainPanel.SetActive(false);
            roosterPanel.SetActive(true);
        }

        public void OnPlayGame()
        {
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
