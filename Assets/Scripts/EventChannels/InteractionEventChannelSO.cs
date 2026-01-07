using System;
using UnityEngine;

namespace EventChannels
{
    [CreateAssetMenu(fileName = "InteractionEventChannelSO", menuName = "Events/Interaction Event Channel")]
    public class InteractionEventChannelSO : ScriptableObject
    {
        public Action OnEventRaised;

        public void RaiseEvent()
        {
            OnEventRaised?.Invoke();
        }
    }
}
