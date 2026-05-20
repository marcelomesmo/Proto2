using System.Collections.Generic;
using Core.Enum;
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
        private MatchSceneController _sceneController;
        
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

            _sceneController = Object.FindFirstObjectByType<MatchSceneController>();

            if (_sceneController == null)
            {
                Debug.LogError("[GameMatchLifecycleHandler] GameMatchLifecycleHandler: scene controller not found");
                return;
            }

            BindPresentation();
        }

        public void HandleMatchEnded(GameController gameController, MatchEndReason reason)
        {
            // Intentionally empty for now.
            // Later: difficulty progression, choose mutators, seed RNG, etc.

            if (gameController.MatchStats is not GameMatchStats stats)
            {
                Debug.LogError(
                    $"Expected GameMatchStats but got: " +
                    $"{gameController.MatchStats?.GetType()}");

                return;
            }

            _sceneController.EndOfLevelPanel.Show(reason, stats, gameController.MatchRuntime.ElapsedTime);
        }

        public void ConfirmExit()
        {
            UnbindPresentation();
            
            ServiceLocator
                .Get<GameController>()
                ?.ExitMatch();
        }
        
        private void BindPresentation()
        {
            if (_sceneController.EndOfLevelPanel == null)
                return;

            _sceneController.EndOfLevelPanel.QuitClicked -= ConfirmExit;
            _sceneController.EndOfLevelPanel.QuitClicked += ConfirmExit;
        }
        
        private void UnbindPresentation()
        {
            if (_sceneController.EndOfLevelPanel == null)
                return;

            _sceneController.EndOfLevelPanel.QuitClicked -= ConfirmExit;
        }
    }
}