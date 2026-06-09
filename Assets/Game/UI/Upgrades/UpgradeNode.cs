using Core.Enum;
using Core.Services;
using Core.Services.Meta;
using Core.Services.Save;
using Core.Upgrades;
using Game.Services.Save;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Upgrades
{
    public sealed class UpgradeNode : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("Data")]
        [SerializeField] private UpgradeDefinition upgrade;
        public UpgradeDefinition Definition => upgrade;

        [Header("UI")]
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image costIcon;

        [Header("Visuals")]
        [SerializeField] private Material grayscaleMaterial;
        [SerializeField] private Material normalMaterial;

        private UpgradeTreePanel _treePanel;
        private GameSaveManager _save;
        private UpgradeManager _upgradeManager;
        
        public void Initialize(UpgradeTreePanel treePanel)
        {
            _treePanel = treePanel;
            _save = ServiceLocator.Get<ISaveManager>() as GameSaveManager;
            _upgradeManager = ServiceLocator.Get<GameController>().UpgradeManager;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);

            Refresh();
        }
        
        //
        //  State handling
        //
        public void Refresh()
        {
            if (upgrade == null || _save == null || _upgradeManager == null)
                return;

            int level = _upgradeManager.GetUpgradeLevel(upgrade.upgradeId);
            levelText.text = $"{level} / {upgrade.maxLevel}";
            
            icon.sprite = upgrade.icon;
            icon.material = normalMaterial;//level > 0 ? normalMaterial : grayscaleMaterial;
            
            var state = _upgradeManager.GetState(upgrade.upgradeId);

            switch(state)
            {
                case UpgradeNodeState.Locked:
                    button.interactable = false;
                    costText.text = "locked";
                    costIcon.enabled = false;
                    icon.material = grayscaleMaterial;
                    break;
                case UpgradeNodeState.Affordable:
                case UpgradeNodeState.Available:
                    int cost = _upgradeManager.GetNextUpgradeCost(upgrade.upgradeId);
                    costText.text = cost.ToString();
                    costIcon.enabled = true;
                    button.interactable = _upgradeManager.CanPurchaseUpgrade(upgrade.upgradeId);
                    break;
                case UpgradeNodeState.Maxed:
                    costText.text = "";
                    costIcon.enabled = false;   // Hide cost icon as well
                    button.interactable = false;
                    break;
            }
        }
        
        //
        //  Button handling
        // 
        private void OnClicked()
        {
            if (_upgradeManager.TryPurchaseUpgrade(upgrade.upgradeId))
            {
                Refresh();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _treePanel.ShowTooltip(upgrade);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _treePanel.HideTooltip();
        }
    }
}
