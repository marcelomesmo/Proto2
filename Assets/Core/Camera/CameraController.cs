using Core.Services;
using UnityEngine;

namespace Core.Camera
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
