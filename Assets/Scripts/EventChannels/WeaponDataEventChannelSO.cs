using System;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDataEventChannelSO", menuName = "Events/Weapon Data Event Channel")]
public class WeaponDataEventChannelSO : ScriptableObject
{
    public Action<WeaponData> OnEventRaised;
    public void RaiseEvent(WeaponData data) => OnEventRaised?.Invoke(data);
}
