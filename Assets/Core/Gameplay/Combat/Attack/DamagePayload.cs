using System.Collections.Generic;
using Core.Gameplay.Combat.StatusEffect;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public readonly struct DamagePayload
    {
        // What happened
        public readonly AttackData hitData;                             // null for DOT / effect damage
        public readonly IReadOnlyList<AttackEffectData> effects;        // null for DOT / effect damage
        public readonly Vector2 hitPoint;
        public readonly DamageSource source;
        public readonly int chainDepth;
        // Optional override for non-attack damage (burn, poison, environment, etc.)
        public readonly DamageDefinition? damageOverride;
        
        public int ResolvedDamage =>
            damageOverride?.damage
            ?? hitData?.damage
            ?? 0;
        
        public DamagePayload(
            AttackData hitData,
            IReadOnlyList<AttackEffectData> effects,
            Vector2 hitPoint,
            DamageSource source,
            int chainDepth = 0,
            DamageDefinition? damageOverride = null)
        {
            this.hitData = hitData;
            this.effects = effects;
            this.hitPoint = hitPoint;
            this.source = source;
            this.chainDepth = chainDepth;
            this.damageOverride = damageOverride;
        }
        
        // Factory for damage-over-time / effect-based damage.
        // Explicitly bypasses AttackData and effects.
        public static DamagePayload CreateEffectDamage(
            int damage,
            DamageSource source,
            Vector2 hitPoint)
        {
            return new DamagePayload(
                hitData: null,
                effects: null,
                hitPoint: hitPoint,
                source: source,
                chainDepth: 0,
                damageOverride: new DamageDefinition(damage)
            );
        }
    }
}