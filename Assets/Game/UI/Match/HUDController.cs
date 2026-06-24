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
        [Header("Loot Collected Channel")]
        [SerializeField] private LootCollectedEventChannelSO lootCollectedEventChannel;
        
        [Header("Loot - Gold")]
        [SerializeField] private TextMeshProUGUI lootGoldText;
        private int _totalGold = 0;
        
        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI killText;
        [SerializeField] private TextMeshProUGUI wavesText;
        [SerializeField] private TextMeshProUGUI matchTimer;
        
        //[Header("Stats")]
        //[SerializeField] private TextMeshProUGUI healthText;
        //[SerializeField] private IntIntEventChannelSO healthEventChannel;
        //[SerializeField] private UIProgressBar healthBar;

        /* Boilerplate for Event Channel */
        private void OnEnable()
        {
            lootCollectedEventChannel.OnEventRaised += HandleLootCollected;
            //healthEventChannel.OnEventRaised += UpdateHealthDisplay;

            var gameController = ServiceLocator
                .Get<GameController>();

            if (gameController.MatchStats is not GameMatchStats gameStats) return;
            
            gameStats
                .OnEnemiesKilledChanged += UpdateKillCounter;
            gameStats.
                OnWaveStartedChanged += UpdateWaveCounter;
            gameStats
                .OnMatchTimeChanged += UpdateMatchClock;
        }

        private void OnDisable()
        {
            lootCollectedEventChannel.OnEventRaised -= HandleLootCollected;
            //healthEventChannel.OnEventRaised -= UpdateHealthDisplay;
            
            var gameController = ServiceLocator
                .Get<GameController>();

            if (gameController.MatchStats is not GameMatchStats gameStats) return;
            
            gameStats
                .OnEnemiesKilledChanged -= UpdateKillCounter;
            gameStats.
                OnWaveStartedChanged -= UpdateWaveCounter;
            gameStats
                .OnMatchTimeChanged -= UpdateMatchClock;
        }
        /* End of Boilerplate */
        
        private void HandleLootCollected(LootCollectedPayload payload)
        {
            // payload.lootType tells you what was collected
            // payload.amount tells you how much
        
            switch (payload.lootType)
            {
                case LootType.Gold:
                    UpdateGoldDisplay(payload.amount);
                    break;
                
                case LootType.Unknown:
                    Debug.LogWarning("[HUDController] Received loot of type: " + payload.lootType + ". Was expecting different?");
                    break;
                
                // etc.
            }
        }

        private void UpdateGoldDisplay(int amount)
        {
            _totalGold += amount;
            lootGoldText.text = $"{_totalGold}";
        }
    
        private void UpdateHealthDisplay(int currHealth, int maxHealth)
        {
            //healthText.text = $"{currHealth} / {maxHealth}";
            //healthBar.SetProgress(currHealth / (float)maxHealth);
        }
        
        private void UpdateKillCounter(int count)
        {
            killText.text = count.ToString();
        }
        
        private void UpdateWaveCounter(int count)
        {
            wavesText.text = count.ToString();
        }
        
        private void UpdateMatchClock(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            matchTimer.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
