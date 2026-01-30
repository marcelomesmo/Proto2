using System;
using Core.EventChannels.Payloads;
using UnityEngine;

namespace Core.EventChannels
{
    [CreateAssetMenu(
        fileName = "CharacterStageUpEventChannel",
        menuName = "Events/Character/Stage Up")]
    public class CharacterStageUpEventChannelSO : ScriptableObject
    {
        public Action<CharacterStageUpPayload> OnEventRaised;

        public void RaiseEvent(CharacterStageUpPayload payload)
        {
            OnEventRaised?.Invoke(payload);
        }
    }
}