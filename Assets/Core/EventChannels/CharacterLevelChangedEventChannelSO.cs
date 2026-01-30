using System;
using Core.EventChannels.Payloads;
using UnityEngine;

namespace Core.EventChannels
{
    [CreateAssetMenu(menuName = "Events/Character/Level Changed")]
    public class CharacterLevelChangedEventChannelSO : ScriptableObject
    {
        public Action<CharacterLevelChangedPayload> OnEventRaised;

        public void RaiseEvent(CharacterLevelChangedPayload levelChangedPayload)
        {
            OnEventRaised?.Invoke(levelChangedPayload);
        }
    }
}

