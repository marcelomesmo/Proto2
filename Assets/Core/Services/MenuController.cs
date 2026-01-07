using UnityEngine;

namespace Core.Services
{
    public class MenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;

        private void Awake()
        {
            ShowMain();
        }

        // ---------- MAIN MENU ----------

        public void OnStartGame()
        {
            // Later: check save slots here
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
