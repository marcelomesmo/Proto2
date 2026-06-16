using System.Collections.Generic;
using Core.Services;
using Core.Services.Meta;
using Core.Services.Save;
using Game.Services.Save;
using UnityEngine;

namespace Game.UI.Upgrades
{
    public sealed class UpgradeTreePanel : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private List<UpgradeNode> upgradeButtons;
        [SerializeField] private UpgradeTooltip tooltip;
        [SerializeField] private UpgradeConnectionRenderer connections;

        private GameSaveManager _save;
        private UpgradeManager _upgradeManager;
        
        private void Awake()
        {
            _save = ServiceLocator.Get<ISaveManager>() as GameSaveManager;
            _upgradeManager = ServiceLocator.Get<GameController>().UpgradeManager as UpgradeManager;

            if (_save == null)
            {
                Debug.LogError("[UpgradePanelController] No SaveManager registered.");
                enabled = false;
                return;
            }
            
            if (_upgradeManager == null)
            {
                Debug.LogError("[UpgradePanelController] No UpgradeManager registered.");
                enabled = false;
                return;
            }

            Build();

            _upgradeManager.OnUpgradeLevelChanged += HandleUpgradeChanged;
            _save.OnGoldChanged += HandleGoldChanged;
        }
        
        private void OnDestroy()
        {
            if (_save == null || _upgradeManager == null)
                return;

            _upgradeManager.OnUpgradeLevelChanged -= HandleUpgradeChanged;
            _save.OnGoldChanged -= HandleGoldChanged;
        }

        private void Build()
        {
            foreach (var btn in upgradeButtons)
                btn.Initialize(this);
            
            connections?.Build(upgradeButtons);
        }
        
        private void HandleUpgradeChanged(string id, int level)
        {
            Refresh();
        }

        private void HandleGoldChanged(int gold)
        {
            Refresh();
        }
        
        public void Refresh()
        {
            foreach (var btn in upgradeButtons)
                btn.Refresh();
        }

        public void ShowTooltip(Core.Upgrades.UpgradeDefinition def, RectTransform nodePosition)
        {
            tooltip?.Show(def, nodePosition);
        }

        public void HideTooltip()
        {
            tooltip?.Hide();
        }

        public void ResetUpgrades()
        {
            _upgradeManager.ResetUpgrades();
            Refresh();
        }
    }
}
