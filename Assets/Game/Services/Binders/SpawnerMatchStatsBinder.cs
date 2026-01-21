using Core.Gameplay.Spawner;
using Core.Services;
using Game.Services.Meta;
using UnityEngine;

namespace Game.Services.Binders
{
    public class SpawnerMatchStatsBinder : MonoBehaviour
    {
        [SerializeField] private EntitySpawner spawner;

        private GameMatchStats _stats;

        private void Awake()
        {
            var gameController = ServiceLocator.Get<GameController>();
            _stats = gameController.MatchStats as GameMatchStats;

            if (_stats == null || spawner == null)
                return;

            spawner.OnWaveCompletedSignal += _stats.RegisterWaveCleared;
        }

        private void OnDestroy()
        {
            if (spawner != null && _stats != null)
                spawner.OnWaveCompletedSignal -= _stats.RegisterWaveCleared;
        }
    }
}
