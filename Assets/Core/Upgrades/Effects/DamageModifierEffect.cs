using System.Collections.Generic;
using Core.Gameplay.Combat.Modifiers;
using UnityEngine;

namespace Core.Upgrades.Effects
{
    [CreateAssetMenu(
        fileName = "DamageModifierEffect",
        menuName = "Upgrades/Effects/Damage Modifier")]
    public class DamageModifierEffect : UpgradeEffect
    {
        public List<DamageModifier> modifiers = new();
        
        public override void Apply(UpgradeContext context)
        {
            // No-op (pull model still used)
        }

        public override void Remove(UpgradeContext context)
        {
            // No-op
        }
    }
}
