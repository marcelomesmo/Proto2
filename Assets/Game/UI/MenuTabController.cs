using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed class MenuTabController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject rosterPanel;
        [SerializeField] private GameObject upgradePanel;

        [Header("Buttons/Tabs")] 
        [SerializeField] private Button rosterButton;

        [SerializeField] private Button upgradesButton;

        private void Awake()
        {
            rosterButton.onClick.AddListener(ShowRoster);
            upgradesButton.onClick.AddListener(ShowUpgrades);

            ShowRoster();
        }

        private void ShowRoster()
        {
            rosterPanel.SetActive(true);
            upgradePanel.SetActive(false);
        }

        private void ShowUpgrades()
        {
            rosterPanel.SetActive(false);
            upgradePanel.SetActive(true);
        }
    }
}
