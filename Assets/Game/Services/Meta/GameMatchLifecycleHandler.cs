using System.Collections.Generic;
using Core.Services;
using Core.Services.Meta;
using Core.Services.Save;
using Core.Upgrades.Database;
using Core.Upgrades.Runtime;
using Game.Services.Save;
using UnityEngine;

namespace Game.Services.Meta
{
    // Game-layer always-on match lifecycle hook.
    // Responsibilities:
    // - Decide which upgrades are active for the match (from save)
    // - Resolve UpgradeDefinitions via IUpgradeDatabase
    // - Load runtime upgrades into Core UpgradeManager
    public sealed class GameMatchLifecycleHandler : MonoBehaviour, IMatchLifecycleHandler
    {
        public void OnMatchStart(GameController gameController)
        {
            if (gameController == null)
                return;

            // Save is game-specific (legal here).
            var save = ServiceLocator.Get<ISaveManager>() as GameSaveManager;
            if (save == null)
            {
                // No save -> no upgrades.
                gameController.UpgradeManager.LoadUpgrades(null);
                return;
            }

            // Database is core an interface service (real or null impl).
            if (!ServiceLocator.TryGet<IUpgradeDatabase>(out var db) || db == null)
            {
                // No database -> no upgrades.
                gameController.UpgradeManager.LoadUpgrades(null);
                return;
            }

            var runtime = new List<RuntimeUpgrade>();

            foreach (var p in save.Profile.upgrades)
            {
                if (!p.unlocked)
                    continue;

                // Only purchased upgrades apply during the match.
                if (p.level <= 0)
                    continue;

                if (!db.TryGet(p.id, out var def) || def == null)
                    continue;

                int level = Mathf.Clamp(p.level, 0, def.maxLevel);
                if (level <= 0)
                    continue;

                runtime.Add(new RuntimeUpgrade(def, level));
            }

            // Registers the upgrades loaded in the UpgradeManager.
            gameController.UpgradeManager.LoadUpgrades(runtime);
        }

        public void OnMatchEnd(GameController gameController)
        {
            // Intentionally empty for now.
            // Later: difficulty progression, choose mutators, seed RNG, etc.
        }
    }
}