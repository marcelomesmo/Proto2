using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core.Debugger
{
    public class BaseDebugDisplay : MonoBehaviour
    {
        private InputAction m_ToggleDebugAction;

        [Header("General")]
        public GameObject debugCanvas;
        [Tooltip("Start with Debug visible?")]
        [SerializeField] private bool showDebug = false;

        [Header("Debug display elements")]
        public TMP_Text debugTextElement_1;
        //public TMP_Text debugTextElement_2;
        
        public void Awake()
        {
            _ToggleDebugCanvas(showDebug);
            m_ToggleDebugAction = InputSystem.actions.FindAction("UI/Debug");
        }

        public void ToggleDebugCanvas(bool show)
        {
            if (showDebug != show)
            {
                _ToggleDebugCanvas(show);
            }
        }
        void _ToggleDebugCanvas(bool show)
        {
            debugCanvas.SetActive(show);
            showDebug = show;
        }

        void Update()
        {
            if(m_ToggleDebugAction.WasPressedThisFrame())
                ToggleDebugCanvas(show: !showDebug);

            if(!showDebug)
                return;

            DrawFPSDebug();
            
            //DrawSecondaryDebug();
        }

        private void DrawFPSDebug()
        {
            var sb = new StringBuilder();

            // Example: Display the current frame rate
            sb.Append("FPS: ").Append((1.0f / Time.deltaTime).ToString("F2"));
            
            // Display the debug text in the top left corner of the screen
            debugTextElement_1.text = sb.ToString();
        }
    }
}