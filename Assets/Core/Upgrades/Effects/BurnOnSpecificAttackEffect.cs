using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Core.Upgrades.Effects
{
    [CreateAssetMenu(
        fileName = "BurnOnSpecificAttackEffect",
        menuName = "Upgrades/Effects/Burn On Attack")]
    public sealed class BurnOnSpecificAttackEffect : UpgradeEffect
    {
        [Header("Target")]
        [SerializeField]
        private AttackData targetAttack;

        [Header("Effect")]
        [SerializeField]
        private AttackEffectData burnEffect;

        public override void Apply(UpgradeContext context)
        {
            var attackSystem =
                context.Entity.GetComponent<EntityAttackSubsystem>();

            attackSystem?.RegisterRuntimeEffect(
                targetAttack,
                burnEffect);
        }
        
        public override void Remove(UpgradeContext context)
        {
            var attackSystem =
                context.Entity.GetComponent<EntityAttackSubsystem>();

            attackSystem?.UnregisterRuntimeEffect(
                targetAttack,
                burnEffect);
        }
    }
}
