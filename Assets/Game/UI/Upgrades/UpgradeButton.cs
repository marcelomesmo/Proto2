using Core.Services;
using Core.Services.Save;
using Core.Upgrades;
using Game.Services.Save;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Upgrades
{
    public sealed class UpgradeButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("Data")]
        [SerializeField] private UpgradeDefinition upgrade;

        [Header("UI")]
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI costText;

        [Header("Visuals")]
        [SerializeField] private Material grayscaleMaterial;
        [SerializeField] private Material normalMaterial;

        private UpgradePanelController _panel;
        private GameSaveManager _save;
        
        public void Initialize(UpgradePanelController panel)
        {
            _panel = panel;
            _save = ServiceLocator.Get<ISaveManager>() as GameSaveManager;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);

            Refresh();
        }
        
        public void Refresh()
        {
            if (upgrade == null || _save == null)
                return;

            icon.sprite = upgrade.icon;

            int level = _save.GetUpgradeLevel(upgrade.upgradeId);
            bool unlocked = _save.IsUpgradeUnlocked(upgrade.upgradeId);

            levelText.text = $"{level} / {upgrade.maxLevel}";

            if (!unlocked)
            {
                button.interactable = false;
                costText.text = "locked";
                icon.material = grayscaleMaterial;
                return;
            }

            if (level >= upgrade.maxLevel)
            {
                costText.text = "";
                button.interactable = false;
            }
            else
            {
                int cost = _save.GetNextUpgradeCost(upgrade.upgradeId);
                costText.text = cost.ToString();
                button.interactable = _save.CanPurchaseUpgrade(upgrade.upgradeId);
            }

            icon.material = level > 0 ? normalMaterial : grayscaleMaterial;
        }
        
        private void OnClicked()
        {
            if (_save.TryPurchaseUpgrade(upgrade.upgradeId))
            {
                Refresh();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _panel.ShowTooltip(upgrade);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _panel.HideTooltip();
        }
    }
}
