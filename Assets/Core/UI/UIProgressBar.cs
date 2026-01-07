using UnityEngine;
using UnityEngine.UI;

namespace Core.UI
{
    public class UIProgressBar : MonoBehaviour
    {
        public Image fillImage;
        public float max = 1f;

        public void SetProgress(float current)
        {
            fillImage.fillAmount = current / max;
        }
    }
}
