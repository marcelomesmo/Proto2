using System;
using UnityEngine;

namespace Core.EventChannels
{
    [CreateAssetMenu(fileName = "IntFloatEventChannelSO", menuName = "Events/Int Float Event Channel")]
    public class IntFloatEventChannelSO : ScriptableObject
    {
        public Action<int, float> OnEventRaised;

        public void RaiseEvent(int intValue, float floatValue)
        {
            OnEventRaised?.Invoke(intValue, floatValue);
        }
    }
}
