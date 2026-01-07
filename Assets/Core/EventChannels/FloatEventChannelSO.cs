using System;
using UnityEngine;

namespace Core.EventChannels
{
    [CreateAssetMenu(fileName = "FloatEventChannelSO", menuName = "Events/Float Event Channel")]
    public class FloatEventChannelSO : ScriptableObject
    {
        public Action<float> OnEventRaised;

        public void RaiseEvent(float currentValue)
        {
            OnEventRaised?.Invoke(currentValue);
        }
    }
}