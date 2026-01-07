using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    public class MusicPlayer : MonoBehaviour
    {
        private AudioSource _source;

        public void Initialize(AudioMixerGroup mixerGroup)
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.loop = true;
            _source.playOnAwake = false;
            _source.outputAudioMixerGroup = mixerGroup;
            _source.spatialBlend = 0f;
        }

        public void Play(AudioClip clip)
        {
            if (_source.clip == clip)
                return;

            _source.clip = clip;
            _source.Play();
        }

        public void Stop()
        {
            _source.Stop();
        }
    }
}