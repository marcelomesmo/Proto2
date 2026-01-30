using System;
using Core.EventChannels.Payloads;
using UnityEngine;

namespace Core.EventChannels
{
    [CreateAssetMenu(menuName = "Events/Character/Level Up")]
    public class CharacterLevelUpEventChannelSO : ScriptableObject
    {
        public Action<CharacterLevelUpPayload> OnEventRaised;

        public void RaiseEvent(CharacterLevelUpPayload levelUpPayload)
        {
            OnEventRaised?.Invoke(levelUpPayload);
        }
    }
}

