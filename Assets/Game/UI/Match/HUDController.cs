using Core.Services;
using Game.Enum;
using Game.EventChannels;
using Game.Services.Meta;
using TMPro;
using UnityEngine;

namespace Game.UI.Match
{
    public class HUDController : MonoBehaviour
    {
        [Header("Progression")]
        [SerializeField] private StageRuntimeController stageRuntime;
        
        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI killText;
        //[SerializeField] private TextMeshProUGUI wavesText; // DEPRECATED
        [SerializeField] private TextMeshProUGUI matchTimer;
        [SerializeField] private TextMeshProUGUI stageLevelText;

        /* Boilerplate for Event Channel */
        private void OnEnable()
        {
            var gameController = ServiceLocator.Get<GameController>();

            if (gameController.MatchStats is GameMatchStats gameStats)
            {
                gameStats.OnEnemiesKilledChanged += UpdateKillCounter;
                //gameStats.OnWaveStartedChanged += UpdateWaveCounter;
                gameStats.OnMatchTimeChanged += UpdateMatchClock;
            }
            
            if (stageRuntime != null)
                stageRuntime.OnLevelStarted += UpdateStageDisplay;
        }

        private void OnDisable()
        {
            var gameController = ServiceLocator
                .Get<GameController>();

            if (gameController.MatchStats is GameMatchStats gameStats)
            {
                gameStats.OnEnemiesKilledChanged -= UpdateKillCounter;
                //gameStats.OnWaveStartedChanged -= UpdateWaveCounter;
                gameStats.OnMatchTimeChanged -= UpdateMatchClock;
            }
            
            if (stageRuntime != null)
                stageRuntime.OnLevelStarted -= UpdateStageDisplay;
        }
        /* End of Boilerplate */
        
        private void UpdateKillCounter(int count)
        {
            killText.text = count.ToString();
        }
        
        // DEPRECATED
        //private void UpdateWaveCounter(int count)
        //{
        //    wavesText.text = count.ToString();
        //}
        
        private void UpdateStageDisplay(int stageIndex, int levelIndex)
        {
            stageLevelText.text = $"Stage {stageIndex + 1}-{levelIndex + 1}";
        }
        
        private void UpdateMatchClock(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            matchTimer.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
