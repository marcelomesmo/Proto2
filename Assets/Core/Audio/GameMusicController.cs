using Core.Services;
using UnityEngine;

namespace Core.Audio
{
    public class GameMusicController : MonoBehaviour
    {
        [SerializeField] private AudioClip menuMusic;       // Should this be here?
        [SerializeField] private AudioClip explorationMusic;
        [SerializeField] private AudioClip combatMusic;

        private void Start()
        {
            AudioManager.Instance.Music.Play(explorationMusic);
        }

        public void EnterCombat()
        {
            AudioManager.Instance.Music.Play(combatMusic);
        }
        
        public void EnterLevel()
        {
            AudioManager.Instance.Music.Play(combatMusic);
        }
        
        public void EnterMenu()
        {
            AudioManager.Instance.Music.Play(menuMusic);
        }
    }
}
