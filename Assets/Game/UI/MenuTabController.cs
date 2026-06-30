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
        
        [Header("Visuals")]
        [SerializeField] private Material grayscaleMaterial;
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Image rosterIcon;
        [SerializeField] private Image upgradeIcon;

        private void Awake()
        {
            rosterButton.onClick.AddListener(ShowRoster);
            upgradesButton.onClick.AddListener(ShowUpgrades);

            ShowRoster();
        }

        private void ShowRoster()
        {
            rosterPanel.SetActive(true);
            rosterIcon.material = normalMaterial;
            upgradePanel.SetActive(false);
            upgradeIcon.material = grayscaleMaterial;
        }

        private void ShowUpgrades()
        {
            rosterPanel.SetActive(false);
            rosterIcon.material = grayscaleMaterial;
            upgradePanel.SetActive(true);
            upgradeIcon.material = normalMaterial;
        }
    }
}
