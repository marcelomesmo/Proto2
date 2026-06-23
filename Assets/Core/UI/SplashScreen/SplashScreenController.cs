using System.Collections;
using Core.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.SplashScreen
{
    public class SplashScreenController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup logoGroup;
        [SerializeField] private CanvasGroup fadeOverlay;
        [SerializeField] private Slider loadingBar;
        [SerializeField] private float minDisplayTime = 2f;

        private IEnumerator Start()
        {
            // Make sure overlay starts fully transparent
            if (fadeOverlay)
                fadeOverlay.alpha = 0f;
            
            float elapsed = 0f;

            // Wait for min display time / loading
            while (elapsed < minDisplayTime)
            {
                elapsed += Time.deltaTime;

                if (loadingBar)
                    loadingBar.value = Mathf.Clamp01(elapsed / minDisplayTime);

                // Fade in from alpha = 0 if logoGroup is set
                if (logoGroup)
                    logoGroup.alpha = Mathf.MoveTowards(
                        logoGroup.alpha, 1f, Time.deltaTime * 1.5f);

                yield return null;
            }
            
            // Fade to black if fadeOverlay is set
            while (fadeOverlay && fadeOverlay.alpha < 1f)
            {
                fadeOverlay.alpha = Mathf.MoveTowards(
                    fadeOverlay.alpha, 1f, Time.deltaTime * 2f);
                yield return null;
            }

            SceneLoader.LoadMenu();
        }
    }
}