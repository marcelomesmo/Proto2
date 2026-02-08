using Core.Gameplay.Spawner;
using Core.Services;
using Game.Services.Meta;
using UnityEngine;

namespace Game.Services.Binders
{
    public class SpawnerMatchStatsBinder : MonoBehaviour
    {
        [SerializeField] private EntitySpawner spawner;

        private GameMatchStats _matchStats;

        private void Awake()
        {
            var gameController = ServiceLocator.Get<GameController>();
            _matchStats = gameController.MatchStats as GameMatchStats;

            if (_matchStats == null || spawner == null)
                return;

            spawner.OnWaveCompletedSignal += _matchStats.RegisterWaveCleared;
        }

        private void OnDestroy()
        {
            if (spawner != null && _matchStats != null)
                spawner.OnWaveCompletedSignal -= _matchStats.RegisterWaveCleared;
        }
    }
}
