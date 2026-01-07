using Core.Audio.Data;
using Core.Services;
using UnityEngine;

namespace Core.Audio
{
    public class EntityAudioEmitter : MonoBehaviour
    {
        public void Play(AudioEvent audioEvent)
        {
            AudioManager.Instance.Play(audioEvent, transform.position);
        }
    }
}
