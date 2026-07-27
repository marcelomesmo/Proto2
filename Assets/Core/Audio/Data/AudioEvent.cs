using UnityEngine;
using UnityEngine.Audio;

namespace Core.Audio.Data
{
    public enum AudioBus
    {
        Music,
        SFX,
        UI,
        Ambient
    }
    
    [CreateAssetMenu(fileName = "Audio Event", menuName = "Audio/Audio Event")]
    public class AudioEvent : ScriptableObject
    {
        [Header("Source")]
        public AudioClip clip;
        public AudioClipSet clipSet;
        
        [Header("Routing")]
        public AudioBus bus;
        public AudioMixerGroup mixerGroup;
        
        [Header("Volume")]
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0f, 0.3f)] public float volumeRandomness = 0.05f;
        
        [Header("Pitch")]
        [Range(0.5f, 2f)] public float pitch = 1f;
        [Range(0f, 0.3f)] public float pitchRandomness = 0.05f;

        [Header("Spatial")]
        [Range(0f, 1f)] public float spatialBlend = 0.7f;
        public float minDistance = 1f;
        public float maxDistance = 12f;

        [Header("Priority")]
        [Range(0, 256)] public int priority = 128;
    }
}
