using System.Collections.Generic;
using Core.Gameplay.Combat.Modifiers;
using UnityEngine;

namespace Core.Upgrades
{
    [CreateAssetMenu(menuName = "Upgrades/Upgrade Definition")]
    public class UpgradeDefinition : ScriptableObject
    {
        [Header("Damage Modifiers")]
        public List<DamageModifier> damageModifiers = new();
    }
}
