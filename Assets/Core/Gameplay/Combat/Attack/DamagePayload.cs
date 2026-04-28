using System.Collections.Generic;
using Core.Gameplay.Combat.Modifiers;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public readonly struct DamagePayload
    {
        // What happened
        public readonly AttackData hitData;                             // null for DOT / effect damage
        public readonly int baseDamage;                                 // ALWAYS set
        public readonly IReadOnlyList<DamageModifier> modifiers;
        public readonly IReadOnlyList<AttackEffectData> effects;        // null for DOT / effect damage
        public readonly Vector2 hitPoint;
        public readonly DamageSource source;
        public readonly int chainDepth;
        
        public DamagePayload(
            AttackData hitData,
            int baseDamage,
            IReadOnlyList<DamageModifier> modifiers,
            IReadOnlyList<AttackEffectData> effects,
            Vector2 hitPoint,
            DamageSource source,
            int chainDepth = 0)
        {
            this.hitData = hitData;
            this.baseDamage = baseDamage;
            this.modifiers = modifiers;
            this.effects = effects;
            this.hitPoint = hitPoint;
            this.source = source;
            this.chainDepth = chainDepth;
        }
        
        public int ResolveDamage()
        {
            float value = baseDamage;

            if (modifiers != null)
            {
                // Additive first
                foreach (var mod in modifiers)
                    if (mod.type == ModifierType.Additive)
                        value += mod.value;

                // Multiplicative second
                foreach (var mod in modifiers)
                    if (mod.type == ModifierType.Multiplicative)
                        value *= mod.value;
            }

            return Mathf.Max(0, Mathf.RoundToInt(value));
        }
        
        // Factory for damage-over-time / effect-based damage.
        // Explicitly bypasses AttackData and effects.
        public static DamagePayload CreateEffectDamage(
            int damage,
            DamageSource source,
            Vector2 hitPoint,
            IReadOnlyList<DamageModifier> modifiers)
        {
            return new DamagePayload(
                hitData: null,
                baseDamage: damage,
                modifiers: modifiers,
                effects: null,
                hitPoint: hitPoint,
                source: source
            );
        }
    }
}