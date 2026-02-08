using System;
using Game.Enum;
using UnityEngine;

namespace Game.EventChannels
{
    // Payload for loot collection events.
    // Contains both the type of loot and the amount collected.
    [Serializable]
    public struct LootCollectedPayload
    {
        public LootType lootType;
        public int amount;

        public LootCollectedPayload(LootType lootType, int amount)
        {
            this.lootType = lootType;
            this.amount = amount;
        }
    }

    // ScriptableObject event channel for loot collection.
    // Allows decoupled communication between loot collection, UI, and stats tracking.
    [CreateAssetMenu(fileName = "LootCollectedEventChannelSO", menuName = "Events/Game/Loot Collected Event Channel")]
    public class LootCollectedEventChannelSO : ScriptableObject
    {
        // Event raised when loot is collected.
        // Subscribe to this to react to loot collection (UI updates, stats tracking, etc.)
        public Action<LootCollectedPayload> OnEventRaised;
        
        // Raise the event with explicit loot type and amount parameters.
        public void RaiseEvent(LootType lootType, int amount)
        {
            OnEventRaised?.Invoke(new LootCollectedPayload(lootType, amount));
        }
        
        // Raise the event with a pre-constructed payload.
        public void RaiseEvent(LootCollectedPayload payload)
        {
            OnEventRaised?.Invoke(payload);
        }
    }
}
