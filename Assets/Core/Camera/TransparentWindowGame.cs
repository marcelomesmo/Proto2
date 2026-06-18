using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

namespace Core.Camera
{
    public class TransparentWindowGame : MonoBehaviour
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [DllImport("TransparentWindowPlugin")]
        static extern void InitializeWindow(
            IntPtr hwnd,
            bool topMost,
            bool borderless,
            int width,
            int height
        );

        [DllImport("TransparentWindowPlugin")]
        static extern void UpdateAlphaFrame(
            IntPtr pixels,
            int width,
            int height
        );
        
        [DllImport("TransparentWindowPlugin")]
        static extern void ResizeWindow(
            int width,
            int height
        );
        
        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();
        
        [DllImport("TransparentWindowPlugin")]
        static extern void SetWindowVisible(bool visible);
        
        [DllImport("TransparentWindowPlugin")]
        static extern bool GetWindowVisible();
#endif
        
        [Header("References")]
        [SerializeField] private UnityEngine.Camera mainCamera;

        [Header("UI To Hide In Transparent Mode")]
        [SerializeField] private GameObject[] objectsToHide;

        [Header("Settings")]
        [SerializeField] private bool borderless = true;
        [SerializeField] private bool pinToTop = false;
        
        private RenderTexture renderTexture;
        private Texture2D readTexture;

        private byte[] frameBuffer;
        
        void Start()
        {
            // Set up render textures immediately (no window calls yet)
            int width = Mathf.RoundToInt(mainCamera.pixelRect.width);
            int height = Mathf.RoundToInt(mainCamera.pixelRect.height);

            renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();

            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0, 0, 0, 0);
            mainCamera.allowHDR = false;

            readTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            frameBuffer = new byte[width * height * 4];

            foreach (GameObject obj in objectsToHide)
                obj.SetActive(false);

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            StartCoroutine(InitializeWindowNextFrame(width, height));
#endif
        }
        
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        private IEnumerator InitializeWindowNextFrame(int width, int height)
        {
            yield return null; // wait one frame for Unity's input system to settle
        
            IntPtr hwnd = GetActiveWindow();
            InitializeWindow(hwnd, pinToTop, borderless, width, height);
        
            mainCamera.targetTexture = renderTexture; // attach only after window is configured
        }
#endif
        
        void LateUpdate()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            if (renderTexture == null)
                return;
            
            RenderTexture.active = renderTexture;

            readTexture.ReadPixels(
                new Rect(
                    0,
                    0,
                    renderTexture.width,
                    renderTexture.height
                ),
                0,
                0,
                false
            );
            
            readTexture.Apply(false, false);

            var raw = readTexture.GetRawTextureData<byte>();

            raw.CopyTo(frameBuffer);
            
            int width = renderTexture.width;
            int height = renderTexture.height;
            
            // RGBA -> BGRA
            /*for(int i=0; i< frameBuffer.Length; i+=4)
            {
                byte r = frameBuffer[i];
                frameBuffer[i] = frameBuffer[i+2];
                frameBuffer[i+2] = r;
            }*/
            
            // flip vertically
            int rowSize = width * 4;

            byte[] flipped = new byte[frameBuffer.Length];
            
            // premultiply alpha
            for(int y=0; y< height; y++)
            {
                Buffer.BlockCopy(
                    frameBuffer,
                    y * rowSize,
                    flipped,
                    (height - y - 1) * rowSize,
                    rowSize
                );
            }
            
            // Convert flipped RGBA -> premultiplied BGRA
            for(
                int i = 0;
                i < flipped.Length;
                i += 4)
            {
                byte r = flipped[i];
                byte g = flipped[i+1];
                byte b = flipped[i+2];
                byte a = flipped[i+3];

                flipped[i] =
                    (byte)(b * a / 255);

                flipped[i+1] =
                    (byte)(g * a / 255);

                flipped[i+2] =
                    (byte)(r * a / 255);

                flipped[i+3] =
                    a;
            }

            GCHandle handle =
                GCHandle.Alloc(
                    flipped,
                    GCHandleType.Pinned
                );

            UpdateAlphaFrame(
                handle.AddrOfPinnedObject(),
                width,
                height
            );

            handle.Free();
#endif
        }
        
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
    [DllImport("TransparentWindowPlugin")]
    static extern void ShutdownWindow();
#endif
        
        void OnDestroy()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            // If this texture is still active, clear it
            if (RenderTexture.active == renderTexture)
                RenderTexture.active = null;

            // CRITICAL: detach before releasing, or Unity's render thread stalls
            if (mainCamera != null)
                mainCamera.targetTexture = null;

            ShutdownWindow();
#endif
            
            if(renderTexture != null)
            {
                renderTexture.Release();
                renderTexture = null;
            }
        }
        
        /*
         This is dangerous without a tray icon as it completely hides the game.
         
         public void ToggleVisibility()
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            bool isVisible = GetWindowVisible();
            SetWindowVisible(!isVisible);
#endif
        }
        */
        
        /*public void ToggleTransparency()
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
        }*/
    }
}