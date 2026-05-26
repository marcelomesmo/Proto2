using Core.Gameplay.Spawner;
using Core.Services;
using UnityEngine;

namespace Game.Services.Binders
{
    public sealed class SpawnerGameControllerBinder : MonoBehaviour
    {
        [SerializeField] private EntitySpawnerRuntimeController spawner;

        private GameController _gameController;

        private void Awake()
        {
            _gameController = ServiceLocator.Get<GameController>();

            if (_gameController == null || spawner == null)
                return;

            spawner.OnAllWavesCompletedSignal += HandleAllWavesCompleted;
        }

        private void OnDestroy()
        {
            if (spawner != null)
                spawner.OnAllWavesCompletedSignal -= HandleAllWavesCompleted;
        }

        private void HandleAllWavesCompleted()
        {
            _gameController.OnGameVictory();
        }
    }
}
