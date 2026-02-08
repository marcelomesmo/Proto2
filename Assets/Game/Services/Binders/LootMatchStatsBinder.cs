using Core.Services;
using Game.EventChannels;
using Game.Services.Meta;
using UnityEngine;

namespace Game.Services.Binders
{
    // Binds loot collection events to GameMatchStats tracking.
    // This component listens to the LootCollectedEventChannelSO and automatically
    // registers collected loot in the match statistics.
    // 
    // Setup:
    // 1. Add this component to a GameObject in your game scene (e.g., GameManager)
    // 2. Assign the LootCollectedEventChannelSO asset to the inspector field
    // 3. The binder will automatically connect to GameMatchStats on Start
    public class LootMatchStatsBinder : MonoBehaviour
    {
        [Header("Event Channels")]
        [Tooltip("The ScriptableObject event channel that broadcasts when loot is collected")]
        [SerializeField] private LootCollectedEventChannelSO lootCollectedChannel;
        
        private GameMatchStats _matchStats;

        private void Start()
        {
            // Get GameMatchStats from GameController
            var gameController = ServiceLocator.Get<GameController>();
            if (gameController?.MatchStats is GameMatchStats stats)
            {
                _matchStats = stats;
                Debug.Log("[LootMatchStatsBinder] Successfully connected to GameMatchStats.");
            }
            else
            {
                Debug.LogWarning("[LootMatchStatsBinder] GameMatchStats not found in GameController. Loot tracking will be disabled.");
            }
        }

        private void OnEnable()
        {
            if (lootCollectedChannel != null)
                lootCollectedChannel.OnEventRaised += HandleLootCollected;
            else
                Debug.LogWarning("[LootMatchStatsBinder] LootCollectedEventChannelSO is not assigned in inspector!", this);
        }

        private void OnDisable()
        {
            if (lootCollectedChannel != null)
                lootCollectedChannel.OnEventRaised -= HandleLootCollected;
        }
        
        // Called when loot is collected anywhere in the game.
        // Forwards the event to GameMatchStats for tracking.
        private void HandleLootCollected(LootCollectedPayload payload)
        {
            if (_matchStats != null)
                _matchStats.RegisterLootCollected(payload.lootType, payload.amount);
        }
    }
}
