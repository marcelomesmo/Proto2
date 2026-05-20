using UnityEngine;
using UnityTransparentWindow;

#if UNITY_STANDALONE_WIN
using System;
using System.Collections;
using System.Runtime.InteropServices;
#endif

namespace Core.Camera
{
    public class TransparentWindowGame : MonoBehaviour
    {
#if UNITY_STANDALONE_WIN
        
        [Header("References")]
        [SerializeField] private UnityEngine.Camera mainCamera;

        [Header("UI To Hide In Transparent Mode")]
        [SerializeField] private GameObject[] objectsToHide;

        [Header("Settings")]
        [SerializeField] private bool borderless = true;
        [SerializeField] private bool pinToTop = false;
        
        private WindowTransparency transparentWindow;

        private bool transparencyEnabled = false;

        private Color normalCameraColor;
        private CameraClearFlags normalClearFlags;

        void Start()
        {
            normalCameraColor = mainCamera.backgroundColor;
            normalClearFlags = mainCamera.clearFlags;
            
            transparentWindow = new WindowTransparency(pinToTop, borderless);
        }

        void Update()
        {
            if (transparencyEnabled && transparentWindow != null)
            {
                transparentWindow.UpdateWindowTransparency();
            }
        }
        
        public void ToggleTransparency()
        {
            if (transparencyEnabled)
            {
                DisableTransparencyMode();
            }
            else
            {
                EnableTransparencyMode();
            }
        }
        
        private void EnableTransparencyMode()
        {
            transparencyEnabled = true;
            
            // Hide UI/background
            foreach (GameObject obj in objectsToHide)
            {
                obj.SetActive(false);
            }
            
            // Set green chroma background
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.green;
            
            // Enable transparency
            transparentWindow.SetWindowAttributes(ColorToUint(Color.green), 0);
        }
        
        private void DisableTransparencyMode()
        {
            transparencyEnabled = false;

            // Restore UI
            foreach (GameObject obj in objectsToHide)
            {
                obj.SetActive(true);
            }

            // Restore camera
            mainCamera.backgroundColor = normalCameraColor;
            mainCamera.clearFlags = normalClearFlags;
            
            // IMPORTANT:
            // Set transparency color to something unused
            transparentWindow.SetWindowAttributes(0x123456, 0);
        }

        private uint ColorToUint(Color color)
        {
            byte r = (byte)(color.r * 255);
            byte g = (byte)(color.g * 255);
            byte b = (byte)(color.b * 255);

            return (uint)((b << 16) | (g << 8) | r);
        }
    }
    
#endif
}