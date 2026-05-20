using UnityEngine;

namespace Core.Camera
{
    public class TransparentWindowButtonToggle : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            bool supported =
                Application.platform == RuntimePlatform.WindowsPlayer;

            gameObject.SetActive(supported);
        }
    }
}
