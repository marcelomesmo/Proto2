using System;
using Core.Services.Meta;
using UnityEngine;

namespace Game.Services.Meta
{
    public sealed class GameMatchStats : MatchStats
    {
        public int EnemiesKilled { get; private set; }
        public int WavesCleared { get; private set; }
       
        public event Action<int> OnEnemiesKilledChanged;
        public event Action<int> OnWaveClearedChanged;  // todo: add to analytics screen? end of level ui?
        public event Action<float> OnMatchTimeChanged;
        
        public override void OnMatchStart()
        {
            EnemiesKilled = 0;
            WavesCleared = 0;
        }
        
        public override void OnMatchTimeUpdated(float time)
        {
            OnMatchTimeChanged?.Invoke(time);
        }
        
        public void RegisterEnemyKilled()
        {
            EnemiesKilled++;
            OnEnemiesKilledChanged?.Invoke(EnemiesKilled);
        }
        public void RegisterWaveCleared()
        {
            WavesCleared++;
            OnWaveClearedChanged?.Invoke(WavesCleared);
        }
    }
}
