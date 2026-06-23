using Core.Services;
using UnityEngine;

namespace Game.Audio
{
    public class MatchMusicHandler : MonoBehaviour
    {
        [Header("Match Music")]
        [SerializeField] private AudioClip baseMusic;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip defeatMusic;

        [Header("Combat Music (optional)")]
        [SerializeField] private AudioClip bossMusic1;
        [SerializeField] private AudioClip bossMusic2;
        [SerializeField] private AudioClip bossMusic3;

        private AudioManager _audio;
        private GameController _game;

        private void Start()
        {
            _audio = ServiceLocator.Get<AudioManager>();
            _game  = ServiceLocator.Get<GameController>();

            _game.OnMatchStarted += HandleMatchStarted;
            _game.OnMatchVictory += HandleMatchVictory;
            _game.OnMatchDefeat  += HandleMatchDefeat;
        }

        private void OnDestroy()
        {
            if (_game == null) return;

            _game.OnMatchStarted -= HandleMatchStarted;
            _game.OnMatchVictory -= HandleMatchVictory;
            _game.OnMatchDefeat  -= HandleMatchDefeat;
        }
        
        private void PlayClip(AudioClip clip)
        {
            if (clip != null)
                _audio.Music.Play(clip);
        }

        // --------------------------------------------------
        // Match lifecycle
        // --------------------------------------------------

        private void HandleMatchStarted()
        {
            if (baseMusic != null)
                _audio.Music.Play(baseMusic);
        }

        private void HandleMatchVictory()
        {
            if (victoryMusic != null)
                _audio.Music.Play(victoryMusic);
        }

        private void HandleMatchDefeat()
        {
            if (defeatMusic != null)
                _audio.Music.Play(defeatMusic);
        }

        // --------------------------------------------------
        // Combat state (called by SpawnerBinder or BossController later)
        // --------------------------------------------------

        public void EnterBoss(int id)
        {
            // TODO: Create a SpawnerMusicHandlerBinder in the scene and hook wave triggers to boss spawns.
            
            switch (id)
            {
                case 1:
                    if (bossMusic1 != null)
                        _audio.Music.Play(bossMusic1);
                    break;
                
                case 2:
                    if (bossMusic2 != null)
                        _audio.Music.Play(bossMusic2);
                    break;
                
                case 3:
                    if (bossMusic3 != null)
                        _audio.Music.Play(bossMusic3);
                    break;
                
                default:
                    Debug.Log("[GameMusicBinder] Music for boss " + id + " not found.");
                    break;
            }
        }

        public void ExitBoss()
        {
            // Restore base music when boss dies
            if (baseMusic != null)
                _audio.Music.Play(baseMusic);
        }
    }
}
