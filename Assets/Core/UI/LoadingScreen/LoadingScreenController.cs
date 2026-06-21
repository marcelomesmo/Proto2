using Core.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.LoadingScreen
{
    public class LoadingScreenController : MonoBehaviour
    {
        [SerializeField] private Slider progressBar;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Start()
        {
            SceneLoader.BeginLoadingFromLoadingScene(OnProgress);
        }
        
        private void OnProgress(float value)
        {
            if (progressBar)
                progressBar.value = value;
        }
        
        private void Update()
        {
            // Optional fade-in
            if (canvasGroup)
                canvasGroup.alpha = Mathf.MoveTowards(
                    canvasGroup.alpha,
                    1f,
                    Time.deltaTime * 2f
                );
        }
    }
}
