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
        [Tooltip("Effect per purchased level. Index = level - 1. If shorter than maxLevel, last element is reused.")]
        [SerializeField]
        private StatusEffectData[] burnEffectPerLevel;

        public override void Apply(UpgradeContext context, int level)
        {
            if (context == null || context.Entity == null)
                return;

            if (level <= 0)
                return;

            var attackSubsystem =
                context.Entity.GetComponent<EntityAttackSubsystem>();

            if (attackSubsystem == null)
                return;

            var effect = GetEffectForLevel(level);
            if (effect == null)
                return;

            attackSubsystem.RegisterRuntimeEffect(targetAttack, effect);
        }

        public override void Remove(UpgradeContext context, int level)
        {
            if (context == null || context.Entity == null)
                return;

            if (level <= 0)
                return;

            var attackSubsystem =
                context.Entity.GetComponent<EntityAttackSubsystem>();

            if (attackSubsystem == null)
                return;

            var effect = GetEffectForLevel(level);
            if (effect == null)
                return;

            attackSubsystem.UnregisterRuntimeEffect(targetAttack, effect);
        }

        private StatusEffectData GetEffectForLevel(int level)
        {
            if (burnEffectPerLevel == null || burnEffectPerLevel.Length == 0)
                return null;

            int idx = Mathf.Clamp(level - 1, 0, burnEffectPerLevel.Length - 1);
            return burnEffectPerLevel[idx];
        }
    }
}
