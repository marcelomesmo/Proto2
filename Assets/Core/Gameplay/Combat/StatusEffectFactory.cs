using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.StatusEffect;
using Core.Gameplay.Combat.StatusEffect.Implementations;
using Core.Gameplay.Entity;

namespace Core.Gameplay.Combat
{
    public static class StatusEffectFactory
    {
        public static StatusEffectInstance Create(
            AttackEffectData effect,
            EntityController target,
            AttackSource source)
        {
            return effect.effectType switch
            {
                StatusEffectType.Burn =>
                    new BurnEffectInstance(
                        target,
                        effect,
                        source,
                        target.Stats.burnTag),

                StatusEffectType.Stun =>
                    new StunEffectInstance(
                        target,
                        effect,
                        target.Stats.stunTag),

                StatusEffectType.Slow =>
                    new SlowEffectInstance(
                        target,
                        effect,
                        target.Stats.slowTag),

                _ => null
            };
        }
    }
}