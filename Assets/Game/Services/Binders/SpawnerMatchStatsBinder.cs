using Core.Gameplay.Spawner;
using Core.Services;
using Game.Services.Meta;
using UnityEngine;

namespace Game.Services.Binders
{
    public class SpawnerMatchStatsBinder : MonoBehaviour
    {
        [SerializeField] private EntitySpawnerRuntimeController spawner;

        private GameMatchStats _matchStats;

        private void Awake()
        {
            var gameController = ServiceLocator.Get<GameController>();
            _matchStats = gameController.MatchStats as GameMatchStats;

            if (_matchStats == null || spawner == null)
                return;

            spawner.OnWaveStartedSignal += _matchStats.RegisterWaveStarted;
            spawner.OnWaveCompletedSignal += _matchStats.RegisterWaveCleared;
        }

        private void OnDestroy()
        {
            if (spawner != null && _matchStats != null)
            {
                spawner.OnWaveStartedSignal -= _matchStats.RegisterWaveStarted;
                spawner.OnWaveCompletedSignal -= _matchStats.RegisterWaveCleared;
            }
        }
    }
}
