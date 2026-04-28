using System.Collections.Generic;
using Core.Services;
using Core.Services.Save;
using Game.Services.Save;
using UnityEngine;

namespace Game.UI.Upgrades
{
    public sealed class UpgradePanelController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private List<UpgradeButton> upgradeButtons;
        [SerializeField] private UpgradeTooltip tooltip;

        private GameSaveManager _save;
        
        private void Awake()
        {
            _save = ServiceLocator.Get<ISaveManager>() as GameSaveManager;

            if (_save == null)
            {
                Debug.LogError("[UpgradePanelController] No SaveManager registered.");
                enabled = false;
                return;
            }

            Build();

            _save.OnUpgradeLevelChanged += HandleUpgradeChanged;
            _save.OnGoldChanged += HandleGoldChanged;
        }
        
        private void OnDestroy()
        {
            if (_save == null)
                return;

            _save.OnUpgradeLevelChanged -= HandleUpgradeChanged;
            _save.OnGoldChanged -= HandleGoldChanged;
        }

        private void Build()
        {
            // TODO: REMOVE THIS AND MOVE TO INITIALIZER
            // TEMP test
            _save.UnlockUpgrade("xp_bonus");
            _save.UnlockUpgrade("damage_global_1");
            _save.UnlockUpgrade("fireball_burn");
            _save.UnlockUpgrade("damage_fire_1");
            
            foreach (var btn in upgradeButtons)
                btn.Initialize(this);
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

        public void ShowTooltip(Core.Upgrades.UpgradeDefinition def)
        {
            tooltip?.Show(def);
        }

        public void HideTooltip()
        {
            tooltip?.Hide();
        }
    }
}
