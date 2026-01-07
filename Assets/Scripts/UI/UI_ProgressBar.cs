using UnityEngine;
using UnityEngine.UI;

public class UI_ProgressBar : MonoBehaviour
{
    public Image fillImage;
    public float max = 1f;

    public void SetProgress(float current)
    {
        fillImage.fillAmount = current / max;
    }
}
