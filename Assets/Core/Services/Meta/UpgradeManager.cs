using System;
using Core.Enum;
using Core.Services.Save;
using Core.Upgrades;
using Core.Upgrades.Database;
using Game.Services.Save;

//
// Responsibility:
// - Meta progression manager.
// - Upgrade unlock evaluation.
// - Upgrade purchase handling.
// - Persistent upgrade progression.
//
namespace Core.Services.Meta
{
    public sealed class UpgradeManager
    {
        // TODO: Later, refactor this to decouple Core from GameSaveManager.
        //      Biggest issue now is having to access Profile (which is Game.GameSave)
        //      and currency (which is hard inferred Gold).
        //      Two questions we need to answer is: how we move GameSave to ISaveManager using a Core.ISave,
        //      converting Gold to GetUpgradeCurrency() and SpendGold() to SpendUpgradeCurrency(),
        //      and whether we want to move UpgradeCurrency to ISaveManager or create IUpgradeCurrencyProvider and inject in UpgradeManager.Initialize.
        private GameSaveManager _saveManager;
        private IUpgradeDatabase _database;
        
        public event Action<string, int> OnUpgradeLevelChanged;
        
        public void Initialize(
            ISaveManager saveManager,
            IUpgradeDatabase database)
        {
            if (saveManager == null)
                throw new ArgumentNullException(nameof(saveManager));
            
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            
            _saveManager = saveManager as GameSaveManager;
            _database = database;
        }
        
        //  ----------------------
        //  Handling Upgrades
        //  ----------------------
        
        private UpgradeProgress GetOrCreateUpgrade(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("[UpgradeManager] registered null upgrade ", nameof(id));
            
            foreach (var u in _saveManager.Profile.upgrades)
            {
                if (u.id == id)
                    return u;
            }

            var progress = new UpgradeProgress
            {
                id = id,
                level = 0
            };

            _saveManager.Profile.upgrades.Add(progress);
            
            return progress;
        }

        public int GetUpgradeLevel(string id) =>
            GetOrCreateUpgrade(id).level;
        
        public bool CanPurchaseUpgrade(string upgradeId)
        {
            if (!_database.TryGet(upgradeId, out var definition))
                return false;
            
            if (!IsUpgradeUnlocked(definition))
                return false;
            
            int level = GetUpgradeLevel(upgradeId);
            
            return level < definition.maxLevel;
        }
        
        public bool CanAffordUpgrade(string upgradeId)
        {
            if (!_database.TryGet(upgradeId, out var definition))
                return false;

            int cost = GetNextUpgradeCost(upgradeId);

            if (cost == int.MaxValue)
                return false;

            return _saveManager.Gold >= cost;
        }
        
        public int GetNextUpgradeCost(string upgradeId)
        {
            if (!_database.TryGet(upgradeId, out var definition))
                return int.MaxValue;

            int currentLevel = GetUpgradeLevel(upgradeId);

            if (currentLevel >= definition.maxLevel)
                return int.MaxValue;

            return definition.GetCostForLevel(currentLevel + 1);
        }
        
        public bool TryPurchaseUpgrade(string upgradeId)
        {
            if (!_database.TryGet(upgradeId, out var definition))
                return false;
            
            if (!CanPurchaseUpgrade(upgradeId))
                return false;
            
            var progress = GetOrCreateUpgrade(upgradeId);
            
            int nextLevel = progress.level + 1;
            int cost = definition.GetCostForLevel(nextLevel);

            if (!_saveManager.SpendGold(cost))  // Validates affordability
                return false;
            
            progress.level = nextLevel;
            
            _saveManager.Save();

            OnUpgradeLevelChanged?.Invoke(
                upgradeId,
                progress.level);

            return true;
        }
        
        public UpgradeNodeState GetState(string upgradeId)
        {
            if(!IsUpgradeUnlocked(upgradeId))
                return UpgradeNodeState.Locked;
            
            if(IsUpgradeMaxLevel(upgradeId))
                return UpgradeNodeState.Maxed;
            
            if(CanAffordUpgrade(upgradeId))
                return UpgradeNodeState.Affordable;
            
            return UpgradeNodeState.Available;
        }
        
        //
        //  Upgrade Tree system core
        //
        public bool IsUpgradeUnlocked(string upgradeId)
        {
            if (!_database.TryGet(upgradeId, out var definition))
                return false;

            return IsUpgradeUnlocked(definition);
        }
        public bool IsUpgradeMaxLevel(string upgradeId)
        {
            if (!_database.TryGet(upgradeId, out var definition))
                return false;

            return GetUpgradeLevel(upgradeId) >= definition.maxLevel;
        }
        
        public bool IsUpgradeUnlocked(UpgradeDefinition definition)
        {
            if (definition == null)
                return false;

            var rule = definition.UnlockRule;

            if (rule == null)
                return true;

            if (rule.Requirements.Count == 0)
                return true;

            switch (rule.Mode)
            {
                case RequirementMode.AnyOf:
                    foreach (var requirement in rule.Requirements)
                    {
                        if (MeetsRequirement(requirement))
                            return true;
                    }

                    return false;

                case RequirementMode.AllOf:
                    foreach (var requirement in rule.Requirements)
                    {
                        if (!MeetsRequirement(requirement))
                            return false;
                    }

                    return true;
            }

            return false;
        }
        
        private bool MeetsRequirement(UpgradeRequirement requirement)
        {
            if (requirement?.Upgrade == null)
                return false;

            return GetUpgradeLevel(
                       requirement.Upgrade.upgradeId)
                   >= requirement.RequiredLevel;
        }
    }
}
