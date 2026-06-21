using System.Collections;
using Core.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.SplashScreen
{
    public class SplashScreenController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup logoGroup;
        [SerializeField] private Slider loadingBar;
        [SerializeField] private float minDisplayTime = 2f;

        private IEnumerator Start()
        {
            float elapsed = 0f;

            while (elapsed < minDisplayTime)
            {
                elapsed += Time.deltaTime;

                if (loadingBar)
                    loadingBar.value = Mathf.Clamp01(elapsed / minDisplayTime);

                if (logoGroup)
                    logoGroup.alpha = Mathf.MoveTowards(
                        logoGroup.alpha, 1f, Time.deltaTime * 1.5f);

                yield return null;
            }

            SceneLoader.LoadMenu();
        }
    }
}