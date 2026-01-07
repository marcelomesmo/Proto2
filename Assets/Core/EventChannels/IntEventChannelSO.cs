using System;
using UnityEngine;

namespace Core.EventChannels
{
    [CreateAssetMenu(fileName = "IntEventChannelSO", menuName = "Events/Int Event Channel")]
    public class IntEventChannelSO : ScriptableObject
    {
        public Action<int> OnEventRaised;

        public void RaiseEvent(int currentValue)
        {
            OnEventRaised?.Invoke(currentValue);
        }
    }
}
