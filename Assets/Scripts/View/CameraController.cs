using Services.Manager;
using UnityEngine;

namespace View
{
    public class CameraController : MonoBehaviour
    {
        public GameObject camera;
        
        // Binds the Camera audio listener to the AudioManager on start of a level.
        private void Start()
        {
            AudioManager.Instance?.BindListener(camera.GetComponent<AudioListener>());
        }
    }
}
