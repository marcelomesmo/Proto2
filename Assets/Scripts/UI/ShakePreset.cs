using UnityEngine;

namespace UI
{
    [CreateAssetMenu(menuName = "Feedback/Camera Shake Preset")]
    public class ShakePreset : ScriptableObject
    {
        public float amplitude = 1f;
        public float frequency = 2f;
        public Vector3 velocity = new Vector3(0.4f, 0f, 0f);
    }
}