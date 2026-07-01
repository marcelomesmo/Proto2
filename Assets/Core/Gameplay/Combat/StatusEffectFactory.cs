using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.StatusEffect;
using Core.Gameplay.Combat.StatusEffect.Implementations;
using Core.Gameplay.Entity;

namespace Core.Gameplay.Combat
{
    public static class StatusEffectFactory
    {
        public static StatusEffectInstance Create(
            StatusEffectData effect,
            EntityController target,
            AttackSource source)
        {
            return effect.effectType switch
            {
                StatusEffectType.Burn =>
                    new BurnEffectInstance(
                        target,
                        effect,
                        source),

                StatusEffectType.Stun =>
                    new StunEffectInstance(
                        target,
                        effect),
                
                StatusEffectType.StatModifier =>
                    new StatModifierEffectInstance(
                        target, 
                        effect),

                StatusEffectType.Knockback =>
                    new KnockbackEffectInstance(
                        target,
                        effect,
                        source),
                
                _ => null
            };
        }
    }
}