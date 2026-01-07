using System;
using UnityEngine;

namespace Core.EventChannels
{
    [CreateAssetMenu(fileName = "IntIntEventChannelSO", menuName = "Events/Int Int Event Channel")]
    public class IntIntEventChannelSO : ScriptableObject
    {
        public Action<int, int> OnEventRaised;
        public void RaiseEvent(int currentAmmo, int maxAmmo) => OnEventRaised?.Invoke(currentAmmo, maxAmmo);
    }
}