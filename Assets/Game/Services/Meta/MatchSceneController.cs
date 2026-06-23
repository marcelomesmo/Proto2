using System;
using System.Collections;
using Core.Level;
using Core.Services;
using Game.Entity.Player;
using Game.UI.Match;
using UnityEngine;

namespace Game.Services.Meta
{
    // Match-scene composition root / entry sequencer.
    // Owns references to scene actors and starts them in a deterministic order.
    //
    // Order:
    // 1) (Optional) Entry sequence hook (fade, animation, etc.)
    // 2) GameController.StartMatch() -> triggers GameMatchLifecycleHook.OnMatchStart (loads runtime upgrades, etc.)
    // 3) PlayerPartyController.BeginMatchParty() -> spawns party and applies already-loaded upgrades
    // 4) (Optional) After-start hook (start waves, enable input, etc.)
    public sealed class MatchSceneController : MonoBehaviour
    {
        [Header("Level Config")]
        [SerializeField] private LevelConfig levelConfig;   // If we want to, we can hook level specific stuff here later.
        
        [Header("Scene References")]
        [SerializeField] private PlayerPartyController playerParty;
        
        [Tooltip("Optional delay before starting match (e.g., for fade-in).")]
        [SerializeField] private float startDelaySeconds = 0f;

        [SerializeField] private EndOfLevelPanel endOfLevelPanel;
        public EndOfLevelPanel EndOfLevelPanel => endOfLevelPanel;
        
        public event Action OnBeforeMatchStart;
        public event Action OnAfterMatchStart;

        private bool _started;

        // ?
        private void Reset()
        {
            playerParty = FindFirstObjectByType<PlayerPartyController>();
        }

        private void Start()
        {
            StartCoroutine(StartMatchRoutine());
        }

        /*
            In case we don't want to auto-start on Start(), call:
        public void StartMatch()
        {
            if (_started)
                return;

            _started = true;

            StartCoroutine(StartMatchRoutine());
        }*/

        private IEnumerator StartMatchRoutine()
        {
            if (!_started)
                _started = true;

            if (playerParty == null)
            {
                Debug.LogError("[MatchSceneController] Missing PlayerPartyController reference.", this);
                yield break;
            }

            OnBeforeMatchStart?.Invoke();

            if (startDelaySeconds > 0f)
                yield return new WaitForSeconds(startDelaySeconds);

            var game = ServiceLocator.Get<GameController>();
            if (!game)
            {
                Debug.LogError("[MatchSceneController] GameController service not found.", this);
                yield break;
            }

            // 1) Start match lifecycle (this triggers the GameMatchLifecycleHook and loads runtime upgrades).
            game.StartMatch();

            // 2) Spawn party and apply upgrades (requires upgrades already loaded above).
            playerParty.Initialize();

            OnAfterMatchStart?.Invoke();
        }
    }
}