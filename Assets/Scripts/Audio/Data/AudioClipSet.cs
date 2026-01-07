using UnityEngine;

namespace Audio.Data
{
    [CreateAssetMenu(fileName = "Audio Clip Set", menuName = "Audio/Audio Clip Set")]
    public class AudioClipSet : ScriptableObject
    {
        public AudioClip[] clips;

        public AudioClip GetRandom()
        {
            if (clips == null || clips.Length == 0)
                return null;

            return clips[Random.Range(0, clips.Length)];
        }
    }
}