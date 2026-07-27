using Core.Audio.Data;
using Core.Enum;
using Core.Services;
using Core.Services.Meta;
using Core.Services.Save;
using Core.Upgrades;
using Game.Enum;
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

        [Header("SFX")]
        [SerializeField] private AudioEvent upgradePurchasedSFX;
        
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
        
        // It is also worth hiding the tooltip when a currently hovered node disappears, particularly when resetting upgrades
        private void OnDisable()
        {
            _treePanel?.HideTooltip();
        }
        
        //
        //  State handling
        //
        public void Refresh()
        {
            if (upgrade == null || _save == null || _upgradeManager == null)
                return;

            // Visibility
            bool shouldBeVisible = IsRevealed;

            if (gameObject.activeSelf != shouldBeVisible)
                gameObject.SetActive(shouldBeVisible);

            if (!shouldBeVisible)
                return;
            
            ApplyDefaults();

            int level = _upgradeManager.GetUpgradeLevel(upgrade.UpgradeId);
            int maxLevel = upgrade.maxLevel;
            
            if (level >= 1) 
                bgPurchased.SetActive(true);
            
            //icon.sprite = upgrade.icon;
            icon.material = normalMaterial;//level > 0 ? normalMaterial : grayscaleMaterial;
            
            var state = _upgradeManager.GetState(upgrade.UpgradeId);

            switch(state)
            {
                case UpgradeNodeState.Locked:
                    // This normally cannot occur while revealed, unless a purchased
                    // upgrade later becomes locked due to some external rule.
                    button.interactable = false;
                    icon.material = grayscaleMaterial;
                    break;
                
                case UpgradeNodeState.Affordable:
                case UpgradeNodeState.Available:
                    if(maxLevel > 1) levelText.text = $"{level} / {maxLevel}";
                    // cant interact if out of coins
                    button.interactable = _upgradeManager.CanPurchaseUpgrade(upgrade.UpgradeId);
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
            if (!_upgradeManager.TryPurchaseUpgrade(upgrade.UpgradeId))
                return;
            
            //_treePanel.Refresh(); // Don't need since TryPurchaseUpgrade always invoke OnUpgradeLevelChanged
            
            // SFX, should this be here?
            AudioManager.Instance.Play(upgradePurchasedSFX);
            
            // Force refresh tooltip
            _treePanel.RefreshTooltip(
                upgrade, 
                _upgradeManager.GetUpgradeLevel(upgrade.UpgradeId), 
                _rectTransform);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _treePanel.ShowTooltip(
                upgrade, 
                _upgradeManager.GetUpgradeLevel(upgrade.UpgradeId), 
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
        
        //
        //  Node State
        //
        public bool IsPurchased
        {
            get
            {
                if (upgrade == null || _upgradeManager == null)
                    return false;

                return _upgradeManager.GetUpgradeLevel(upgrade.UpgradeId) >= 1;
            }
        }
        
        public bool IsRevealed
        {
            get
            {
                if (upgrade == null || _upgradeManager == null)
                    return false;

                // Once purchased, the node remains revealed permanently.
                if (_upgradeManager.GetUpgradeLevel(upgrade.UpgradeId) > 0)
                    return true;

                // Available and Affordable are both progression-unlocked.
                return _upgradeManager.GetState(upgrade.UpgradeId) is
                    UpgradeNodeState.Available or
                    UpgradeNodeState.Affordable;
            }
        }

        public UpgradeNodeState CurrentState
        {
            get
            {
                if (upgrade == null || _upgradeManager == null)
                    return UpgradeNodeState.Locked;

                return _upgradeManager.GetState(upgrade.UpgradeId);
            }
        }

        public UpgradeConnectionState GetIncomingConnectionState()
        {
            if (IsPurchased)
                return UpgradeConnectionState.Purchased;

            return CurrentState switch
            {
                UpgradeNodeState.Available => UpgradeConnectionState.Available,
                UpgradeNodeState.Affordable => UpgradeConnectionState.Available,
                _ => UpgradeConnectionState.Inactive
            };
        }
    }
}
