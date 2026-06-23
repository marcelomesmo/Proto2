using System.Collections;
using Core.Enum;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Core.Services
{
    public class SceneTransitionService : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform wipePanel;
        [SerializeField] private CanvasGroup fadePanel;
        
        [Header("Defaults")]
        [SerializeField] private float defaultDuration = 0.4f;
        [SerializeField] private UnityEngine.Camera transitionCamera;
    
        // Cached canvas width — wipe travels this distance off-screen
        private float _screenWidth;

        private void Awake()
        {
            var canvas = GetComponentInChildren<Canvas>();
            _screenWidth = canvas.GetComponent<RectTransform>().rect.width;

            // Start in neutral state — nothing visible
            SetWipeX(-_screenWidth); // Start fully off-screen to the left — invisible by default
            SetFadeAlpha(0f);
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var mainCam = UnityEngine.Camera.main;
            if (mainCam == null)
            {
                Debug.LogWarning("[SceneTransitionService] No Camera.main found in scene.");
                return;
            }

            var cameraData = mainCam.GetUniversalAdditionalCameraData();
            if (!cameraData.cameraStack.Contains(transitionCamera))
                cameraData.cameraStack.Add(transitionCamera);
        }
        
        // --------------------------------------------------
        // Internal API — called by SceneLoader with LevelConfig
        // --------------------------------------------------
        
        // Called by SceneLoader — runs the full cover → load → uncover sequence
        public IEnumerator PlayFullTransition(TransitionStyle style, float duration, 
            IEnumerator loadRoutine)
        {
            // Cover
            yield return StartCoroutine(PlayTransitionIn(style, duration));
    
            // Load
            yield return StartCoroutine(loadRoutine);
    
            // Uncover
            yield return StartCoroutine(PlayTransitionOut(style, duration));
        }
        
        // --------------------------------------------------
        // External API — cinematics, boss fights, etc.
        // --------------------------------------------------
        
        public IEnumerator PlayTransitionIn(TransitionStyle style, float duration)
        {
            yield return style switch
            {
                TransitionStyle.WipeLeftToRight => PlayWipeLeftToRight(duration),
                TransitionStyle.FadeOut => PlayFadeToBlack(duration),
                _                   => null
            };
        }

        public IEnumerator PlayTransitionOut(TransitionStyle style, float duration)
        {
            yield return style switch
            {
                TransitionStyle.WipeLeftToRight => PlayWipeLeftToRightBack(duration),
                TransitionStyle.FadeOut => PlayFadeFromBlack(duration),
                _                   => null
            };
        }
        
        // --------------------------------------------------
        // Wipe
        // --------------------------------------------------
        
        // Covers screen left-to-right (intro)
        private IEnumerator PlayWipeLeftToRight(float duration)
        {
            yield return MoveWipe(-_screenWidth, 0f, duration);
        }
        
        // Uncovers screen going back from right to left (outro)
        private IEnumerator PlayWipeLeftToRightBack(float duration)
        {
            yield return MoveWipe(0f, -_screenWidth, duration);
        }
        
        private IEnumerator MoveWipe(float from, float to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                SetWipeX(Mathf.Lerp(from, to, t));
                yield return null;
            }

            SetWipeX(to);
        }
        
        private void SetWipeX(float x)
        {
            var pos = wipePanel.anchoredPosition;
            pos.x = x;
            wipePanel.anchoredPosition = pos;
        }
        
        // --------------------------------------------------
        // Fade
        // --------------------------------------------------
        
        // Covers screen (fade to black)
        private IEnumerator PlayFadeToBlack(float duration)
        {
            yield return TweenFade(0f, 1f, duration);
        }

        // Uncovers screen (fade from black)
        private IEnumerator PlayFadeFromBlack(float duration)
        {
            yield return TweenFade(1f, 0f, duration);
        }
        
        private IEnumerator TweenFade(float from, float to, float duration)
        {
            float elapsed = 0f;
            SetFadeAlpha(from);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetFadeAlpha(Mathf.Lerp(from, to, t));
                yield return null;
            }

            SetFadeAlpha(to);
        }
        
        private void SetFadeAlpha(float alpha)
        {
            if (fadePanel != null)
                fadePanel.alpha = alpha;
        }
    }
}
