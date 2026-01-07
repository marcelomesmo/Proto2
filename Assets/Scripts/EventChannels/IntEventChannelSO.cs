using System;
using UnityEngine;

[CreateAssetMenu(fileName = "IntEventChannelSO", menuName = "Events/Int Event Channel")]
public class IntEventChannelSO : ScriptableObject
{
    public Action<int> OnEventRaised;

    public void RaiseEvent(int currentValue)
    {
        OnEventRaised?.Invoke(currentValue);
    }
}
