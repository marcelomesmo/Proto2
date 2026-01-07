using Audio.Data;
using Services.Manager;
using UnityEngine;

namespace Audio
{
    public class EntityAudioEmitter : MonoBehaviour
    {
        public void Play(AudioEvent audioEvent)
        {
            AudioManager.Instance.Play(audioEvent, transform.position);
        }
    }
}
