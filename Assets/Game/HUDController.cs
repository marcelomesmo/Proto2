using Core.EventChannels;
using Core.UI;
using TMPro;
using UnityEngine;

namespace UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Wallet")]
        [SerializeField] private TextMeshProUGUI resource1Text;
        [SerializeField] private IntEventChannelSO resource1EventChannel;
        private int _totalResource1Contribution = 0;
        [SerializeField] private TextMeshProUGUI resource2Text;
        [SerializeField] private IntEventChannelSO resource2EventChannel;
        private int _totalResource2Contribution = 0;
        [SerializeField] private TextMeshProUGUI resource3Text;
        [SerializeField] private IntEventChannelSO resource3EventChannel;
        private int _totalResource3Contribution = 0;
    
        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private IntIntEventChannelSO healthEventChannel;
        [SerializeField] private UIProgressBar healthBar;
        [SerializeField] private FloatEventChannelSO sprintEventChannel;
        [SerializeField] private UIProgressBar sprintBar;

        /* Boilerplate for Event Channel */
        private void OnEnable()
        {
            resource1EventChannel.OnEventRaised += UpdateResource1Display;
            resource2EventChannel.OnEventRaised += UpdateResource2Display;
            resource3EventChannel.OnEventRaised += UpdateResource3Display;
            healthEventChannel.OnEventRaised += UpdateHealthDisplay;
            sprintEventChannel.OnEventRaised += UpdateSprintDisplay;
        }

        private void OnDisable()
        {
            resource1EventChannel.OnEventRaised -= UpdateResource1Display;
            resource2EventChannel.OnEventRaised -= UpdateResource2Display;
            resource3EventChannel.OnEventRaised -= UpdateResource3Display;
            healthEventChannel.OnEventRaised -= UpdateHealthDisplay;
            sprintEventChannel.OnEventRaised -= UpdateSprintDisplay;
        }
        /* End of Boilerplate */
        
        private void UpdateResource1Display(int contributionValue)
        {
            _totalResource1Contribution += contributionValue;
            resource1Text.text = $"{_totalResource1Contribution}";
        }
        private void UpdateResource3Display(int contributionValue)
        {
            _totalResource2Contribution += contributionValue;
            resource2Text.text = $"{_totalResource2Contribution}";
        }
        private void UpdateResource2Display(int contributionValue)
        {
            _totalResource3Contribution += contributionValue;
            resource3Text.text = $"{_totalResource3Contribution}";
        }
    
        private void UpdateHealthDisplay(int currHealth, int maxHealth)
        {
            healthText.text = $"{currHealth} / {maxHealth}";
            healthBar.SetProgress(currHealth / (float)maxHealth);
        }

        private void UpdateSprintDisplay(float sprintProgress)
        {
            sprintBar.SetProgress(sprintProgress);
        }
    }
}
