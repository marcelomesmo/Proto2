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
        [SerializeField] private GameObject iconMaxed;

        [Header("Visuals")]
        [SerializeField] private GameObject bgPurchased;
        [SerializeField] private Material grayscaleMaterial;
        [SerializeField] private Material normalMaterial;

        private UpgradeTreePanel _treePanel;
        private GameSaveManager _save;
        private UpgradeManager _upgradeManager;
        
        private RectTransform _rectTransform;
        
        public void Initialize(UpgradeTreePanel treePanel)
        {
            _treePanel = treePanel;
            _save = ServiceLocator.Get<ISaveManager>() as GameSaveManager;
            _upgradeManager = ServiceLocator.Get<GameController>().UpgradeManager;
            
            _rectTransform = transform as RectTransform;
            
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

            ApplyDefaults();

            int level = _upgradeManager.GetUpgradeLevel(upgrade.upgradeId);
            if(level >= 1) bgPurchased.SetActive(true);
            int maxLevel = upgrade.maxLevel;
            
            //icon.sprite = upgrade.icon;
            icon.material = normalMaterial;//level > 0 ? normalMaterial : grayscaleMaterial;
            
            var state = _upgradeManager.GetState(upgrade.upgradeId);

            switch(state)
            {
                case UpgradeNodeState.Locked:
                    button.interactable = false;
                    icon.material = grayscaleMaterial;
                    break;
                case UpgradeNodeState.Affordable:
                case UpgradeNodeState.Available:
                    if(maxLevel > 1) levelText.text = $"{level} / {maxLevel}";
                    // cant interact if out of coins
                    button.interactable = _upgradeManager.CanPurchaseUpgrade(upgrade.upgradeId);
                    break;
                case UpgradeNodeState.Maxed:
                    iconMaxed.SetActive(true);
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
            
                // Force refresh tooltip
                _treePanel.RefreshTooltip(
                    upgrade, 
                    _upgradeManager.GetUpgradeLevel(upgrade.upgradeId), 
                    _rectTransform);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _treePanel.ShowTooltip(
                upgrade, 
                _upgradeManager.GetUpgradeLevel(upgrade.upgradeId), 
                _rectTransform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _treePanel.HideTooltip();
        }

        private void ApplyDefaults()
        {
            levelText.text = "";
            bgPurchased.SetActive(false);
            iconMaxed.SetActive(false);
        }
    }
}
