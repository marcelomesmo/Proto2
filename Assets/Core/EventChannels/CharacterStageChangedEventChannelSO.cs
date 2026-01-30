using System;
using Core.EventChannels.Payloads;
using UnityEngine;

namespace Core.EventChannels
{
    [CreateAssetMenu(
        fileName = "CharacterStageChangedEventChannel",
        menuName = "Events/Character/Stage Changed")]
    public class CharacterStageChangedEventChannelSO : ScriptableObject
    {
        public Action<CharacterStageChangedPayload> OnEventRaised;

        public void RaiseEvent(CharacterStageChangedPayload payload)
        {
            OnEventRaised?.Invoke(payload);
        }
    }
}