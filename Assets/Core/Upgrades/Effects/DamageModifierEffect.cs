using Core.Gameplay.Combat.Modifiers;
using UnityEngine;

namespace Core.Upgrades.Effects
{
    [CreateAssetMenu(
        fileName = "DamageModifierEffect",
        menuName = "Upgrades/Effects/Damage Modifier")]
    public class DamageModifierEffect : UpgradeEffect
    {
        public DamageModifier modifier;
        
        public override void Apply(UpgradeContext context)
        {
            context.Modifiers.AddDamageModifier(modifier);
        }

        public override void Remove(UpgradeContext context)
        {
            context.Modifiers.RemoveDamageModifier(modifier);
        }
    }
}
