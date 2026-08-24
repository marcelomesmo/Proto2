using System;
using System.Collections;
using Core.Enum;
using Core.Gameplay.Spawner;
using Core.Services;
using Core.Services.Manager;
using Core.Services.Save;
using Game.Entity.Player;
using Game.Services.Save;
using Game.UI.Match;
using Game.UI.Roster;
using UnityEngine;

namespace Game.Services.Meta
{
    // Match-scene composition root / entry sequencer.
    // 
    // Responsibilities:
    // - Initialize persistent party state.
    // - Gate gameplay behind first-character selection.
    // - Start the match only after a valid party exists.
    //
    // Order:
    // 1) (Optional) Entry sequence hook (fade, animation, etc.)
    // 2) GameController.StartMatch() -> triggers GameMatchLifecycleHook.OnMatchStart (loads runtime upgrades, etc.)
    // 3) PlayerPartyController.Initialize() -> spawns party and applies already-loaded upgrades
    // 4) (Optional) After-start hook (start waves, enable input, etc.)
    //
    // Current flow:
    //
    // GameScene loads
    //      ↓
    // Initialize PartyManager + runtime PlayerLoadoutData
    //      ↓
    // Party exists?
    //      ├── No  → Show initial character selection
    //      │           ↓
    //      │       Select character
    //      │           ↓
    //      │       Unlock + assign to slot 0
    //      │           ↓
    //      └──────── Start match
    //
    //      └── Yes → Start match directly
    //
    public sealed class MatchSceneController : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private PlayerPartyController playerParty;
        [SerializeField] private StageRuntimeController stageRuntime;
        
        [Header("Persistent Game State")]
        [SerializeField] private CharacterDatabase characterDatabase;
        
        [Header("First Character Selection")]
        [SerializeField] private InitialCharacterSelectionPanel initialCharacterSelectionPanel;
      
        [Header("Gameplay Presentation")]
        [SerializeField] private GameObject gameplayPresentationRoot;
        
        [Tooltip("Optional delay before starting match (e.g., for fade-in).")]
        [SerializeField] private float startDelaySeconds = 0f;

        [SerializeField] private EndOfLevelPanel endOfLevelPanel;
        public EndOfLevelPanel EndOfLevelPanel => endOfLevelPanel;
        
        public event Action OnBeforeMatchStart;
        public event Action OnAfterMatchStart;

        private GameSaveManager _saveManager;
        private PartyManager _partyManager;
        
        private bool _started;
        
        // --------------------------------------------------
        // Unity Lifecycle
        // --------------------------------------------------
        
        private void Reset()
        {
            playerParty =
                FindFirstObjectByType<PlayerPartyController>();

            endOfLevelPanel =
                FindFirstObjectByType<EndOfLevelPanel>();

            initialCharacterSelectionPanel =
                FindFirstObjectByType<InitialCharacterSelectionPanel>();
            
            stageRuntime =
                FindFirstObjectByType<StageRuntimeController>();
        }

        private void Start()
        {
            StartCoroutine(InitializeGameSceneRoutine());
        }

        private void OnDestroy()
        {
            UnbindInitialCharacterSelection();
            UnbindStageRuntime();
        }

        // --------------------------------------------------
        // GameScene Initialization
        // --------------------------------------------------
        
        private IEnumerator InitializeGameSceneRoutine()
        {
            if (!ValidateSceneReferences())
                yield break;
            
            _saveManager = ServiceLocator.Get<ISaveManager>() as GameSaveManager;
            
            if (_saveManager == null)
            {
                Debug.LogError("[MatchSceneController] GameSaveManager service not found.", this);
                yield break;
            }
            
            PlayerLoadoutData loadout = playerParty.Loadout;
            
            if (loadout == null)
            {
                Debug.LogError("[MatchSceneController] PlayerPartyController has no PlayerLoadoutData assigned.", this);
                yield break;
            }
            
            // PartyManager owns the relationship between:
            //
            // GameSave
            //      ↓
            // Persistent party state
            //      ↓
            // Runtime PlayerLoadoutData
            //
            _partyManager = new PartyManager(
                _saveManager,
                loadout,
                characterDatabase);
            
            if (!_partyManager.Initialize())
            {
                Debug.LogError("[MatchSceneController] Failed to initialize party state.", this);
                yield break;
            }
            
            if (!stageRuntime.Initialize(_saveManager))
            {
                Debug.LogError("[MatchSceneController] Failed to initialize Stage runtime.", this);
                yield break;
            }

            BindStageRuntime();
            
            // (ONE-TIME ONLY) First-ever character selection.
            if (!_partyManager.HasConfiguredParty)
            {
                ShowInitialCharacterSelection();
                yield break;
            }
            
            // Safety check:
            // A persistent party exists, so the runtime loadout should contain at least one resolved character.
            // i.e. even if it was the first-time, the character should at least have been assigned to the first slot.
            if (loadout.CurrentPartySize <= 0)
            {
                Debug.LogError("[MatchSceneController] A saved party exists, but the runtime loadout contains no valid characters.", this);
                yield break;
            }
            
            yield return StartAttemptRoutine();
        }
        
        private bool ValidateSceneReferences()
        {
            if (playerParty == null)
            {
                Debug.LogError("[MatchSceneController] Missing PlayerPartyController reference.", this);
                return false;
            }

            if (characterDatabase == null)
            {
                Debug.LogError("[MatchSceneController] Missing CharacterDatabase reference.", this);
                return false;
            }
            
            if (stageRuntime == null)
            {
                Debug.LogError("[MatchSceneController] Missing StageRuntimeController reference.", this);
                return false;
            }

            return true;
        }
        
        // --------------------------------------------------
        // Stage Runtime
        // --------------------------------------------------
        
        private void BindStageRuntime()
        {
            stageRuntime.OnLevelCompleted -= HandleLevelCompleted;
            stageRuntime.OnLevelCompleted += HandleLevelCompleted;
        }

        private void UnbindStageRuntime()
        {
            if (stageRuntime == null)
                return;

            stageRuntime.OnLevelCompleted -= HandleLevelCompleted;
        }
        
        private void HandleLevelCompleted(int completedStageIndex, int completedLevelIndex)
        {
            // The current Level has ended, but the match attempt continues.
            // We therefore DO NOT call GameController.EndMatch() and DO NOT respawn the party.
            if (stageRuntime.TryMoveToNextLevel())
            {
                stageRuntime.StartCurrentLevel();
                return;
            }

            // No next Level exists:
            // final Level of final Stage completed.
            ServiceLocator
                .Get<GameController>()
                ?.OnGameVictory();
        }
        
        // --------------------------------------------------
        // Initial Character Selection
        // --------------------------------------------------

        private void ShowInitialCharacterSelection()
        {
            if (initialCharacterSelectionPanel == null)
            {
                Debug.LogError("[MatchSceneController] The player has no configured party, but no InitialCharacterSelectionPanel is assigned.", this);
                return;
            }
            
            if (gameplayPresentationRoot != null)
                gameplayPresentationRoot.SetActive(false);
            
            BindInitialCharacterSelection();

            initialCharacterSelectionPanel.Show();
        }

        private void BindInitialCharacterSelection()
        {
            initialCharacterSelectionPanel.CharacterSelected -= HandleInitialCharacterSelected;
            initialCharacterSelectionPanel.CharacterSelected += HandleInitialCharacterSelected;
        }

        private void UnbindInitialCharacterSelection()
        {
            if (initialCharacterSelectionPanel == null)
                return;

            initialCharacterSelectionPanel.CharacterSelected -= HandleInitialCharacterSelected;
        }
        
        // Initial selection has one explicit rule:
        // The selected character is unlocked for free and assigned to party slot 0.
        private void HandleInitialCharacterSelected(
            CharacterDefinition definition)
        {
            if (_partyManager == null)
            {
                Debug.LogError("[MatchSceneController] PartyManager has not been initialized.", this);
                return;
            }

            PartyChangeResult result = _partyManager.TryCompleteInitialCharacterSelection(definition);

            if (result != PartyChangeResult.Success)
            {
                Debug.LogError($"[MatchSceneController] Initial character selection failed: {result}.", this);
                return;
            }

            UnbindInitialCharacterSelection();

            initialCharacterSelectionPanel.Hide();
            
            if (gameplayPresentationRoot != null)
                gameplayPresentationRoot.SetActive(true);

            StartCoroutine(StartAttemptRoutine());
        }
        
        // --------------------------------------------------
        // Match Attempt
        // --------------------------------------------------

        private IEnumerator StartAttemptRoutine()
        {
            if (_started)
                yield break;

            _started = true;

            OnBeforeMatchStart?.Invoke();

            if (startDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(startDelaySeconds);
            }

            GameController game = ServiceLocator.Get<GameController>();

            if (game == null)
            {
                Debug.LogError("[MatchSceneController] GameController service not found.", this);
                _started = false;
                yield break;
            }

            // 1. Initialize match-level runtime state.
            // This loads purchased upgrades through GameMatchLifecycleHandler.
            game.StartMatch();

            // 2. Spawn the castle + party and apply the loaded upgrades.
            playerParty.Initialize();
            
            // 3. Only begin enemy spawning once the player's runtime party is completely initialized.
            stageRuntime.StartCurrentLevel();

            OnAfterMatchStart?.Invoke();
        }
        
        
        // --------------------------------------------------
        // Match Lifecycle calls
        // --------------------------------------------------
        
        public void BeginAttemptEnd(MatchEndReason reason)
        {
            // Prevent any additional waves/enemies from spawning
            // while the end sequence is playing.
            // Stop the currently active Level immediately.
            stageRuntime.StopCurrentLevel();
        }
        
        public void HandleDefeat()
        {
            CleanupAttempt();

            // Defeat sends the player back exactly one Level.
            // If we are already at the first Level, TryMoveToPreviousLevel() simply returns false
            // and we replay the first Level.
            stageRuntime.ApplyDefeatFallback();
            
            StartCoroutine(StartAttemptRoutine());
            
            // Later, Defeat can also trivially become:
            //retryPanel.Show();
            // and we move the above code to Retry() in the Retry button
        }
        
        private void CleanupAttempt()
        {
            // First allow scene actors to detach their own references
            // and subscriptions.
            playerParty.EndAttempt();

            // Then return every active entity to its existing pool:
            // player characters, castle and enemies.
            ServiceLocator.Get<EntityPoolManager>()?.ReleaseAll();

            _started = false;
        }
        
        public void HandleVictory(
            GameMatchStats stats,
            float elapsedTime)
        {
            //
            // Victory is terminal for the current implementation.
            //
            // Do not restart.
            //
            EndOfLevelPanel.Show(
                MatchEndReason.Victory,
                stats,
                elapsedTime);
            
            // Later, add anything Victory related here, such as ending the game.
        }
    }
}