using Core.Services;
using Core.Services.Save;
using Game.Enum;
using Game.EventChannels;
using Game.Services.Save;
using UnityEngine;

namespace Game.Loot
{
    //
    // Converts collected game loot into persistent player currency.
    //
    // GameLoot only reports that loot was collected.
    // This controller decides what that means for persistent progression.
    //
    public sealed class LootCurrencyController : MonoBehaviour
    {
        [SerializeField]
        private LootCollectedEventChannelSO lootCollectedEventChannel;

        private GameSaveManager _saveManager;

        private void OnEnable()
        {
            _saveManager = ServiceLocator.Get<ISaveManager>() as GameSaveManager;

            if (_saveManager == null)
            {
                Debug.LogError("[LootCurrencyController] GameSaveManager service not found.", this);
                return;
            }

            if (lootCollectedEventChannel == null)
            {
                Debug.LogError("[LootCurrencyController] LootCollectedEventChannelSO is not assigned.", this);
                return;
            }

            lootCollectedEventChannel.OnEventRaised += HandleLootCollected;
        }

        private void OnDisable()
        {
            if (lootCollectedEventChannel != null)
            {
                lootCollectedEventChannel.OnEventRaised -= HandleLootCollected;
            }
        }

        private void HandleLootCollected(LootCollectedPayload payload)
        {
            if (_saveManager == null)
                return;

            if (payload.amount <= 0)
                return;

            switch (payload.lootType)
            {
                case LootType.Gold:
                    _saveManager.AddGold(payload.amount);
                    break;

                case LootType.Unknown:
                    Debug.LogWarning("[LootCurrencyController] Received unknown loot type.", this);
                    break;

                default:
                    //
                    // Future currencies such as Wood, Stone, etc.
                    //
                    break;
            }
        }
    }
}