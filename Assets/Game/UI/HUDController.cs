using Core.EventChannels;
using Core.Services;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Wallet")]
        [SerializeField] private TextMeshProUGUI lootGoldText;
        [SerializeField] private IntEventChannelSO lootGoldEventChannel;
        private int _totalGold = 0;
        
        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI killText;
        [SerializeField] private TextMeshProUGUI matchTimer;
        
        //[Header("Stats")]
        //[SerializeField] private TextMeshProUGUI healthText;
        //[SerializeField] private IntIntEventChannelSO healthEventChannel;
        //[SerializeField] private UIProgressBar healthBar;

        /* Boilerplate for Event Channel */
        private void OnEnable()
        {
            lootGoldEventChannel.OnEventRaised += UpdateGoldDisplay;
            //healthEventChannel.OnEventRaised += UpdateHealthDisplay;
            
            ServiceLocator
                .Get<GameController>()
                .MatchStats
                .OnEnemiesKilledChanged += UpdateKillCounter;
            
            ServiceLocator
                .Get<GameController>()
                .MatchStats
                .OnMatchTimeUpdated += UpdateMatchClock;
        }

        private void OnDisable()
        {
            lootGoldEventChannel.OnEventRaised -= UpdateGoldDisplay;
            //healthEventChannel.OnEventRaised -= UpdateHealthDisplay;
            
            ServiceLocator
                .Get<GameController>()
                .MatchStats
                .OnEnemiesKilledChanged -= UpdateKillCounter;
            
            ServiceLocator
                .Get<GameController>()
                .MatchStats
                .OnMatchTimeUpdated -= UpdateMatchClock;
        }
        /* End of Boilerplate */
        
        private void UpdateGoldDisplay(int contributionValue)
        {
            _totalGold += contributionValue;
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
        
        private void UpdateMatchClock(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            matchTimer.text = $"{minutes:00}:{seconds:00}";
        }
    }
}
