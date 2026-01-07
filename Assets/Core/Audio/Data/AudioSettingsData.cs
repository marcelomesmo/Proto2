using UnityEngine;

namespace Core.Audio.Data
{
    [CreateAssetMenu(fileName = "Audio Settings Data", menuName = "Audio/Audio Settings")]
    public class AudioSettingsData : ScriptableObject
    {
        [Header("Pooling")]
        public int initialPoolSize = 24;

        [Header("2D Distance Handling")]
        public bool ignoreVerticalDistance = true;
    }
}