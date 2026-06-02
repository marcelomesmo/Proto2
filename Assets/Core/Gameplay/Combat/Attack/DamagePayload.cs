using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Modifiers;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public readonly struct DamagePayload
    {
        // What happened
        public readonly AttackInstance attack;                             // null for DOT / effect damage
        public readonly int baseDamage;                                 // ALWAYS set
        public readonly IReadOnlyList<DamageModifier> modifiers;
        public readonly IReadOnlyList<AttackEffectData> effects;        // null for DOT / effect damage
        public readonly DamageSource source;
        public readonly int chainDepth;
        
        public DamagePayload(
            AttackInstance attack,
            int baseDamage,
            IReadOnlyList<DamageModifier> modifiers,
            IReadOnlyList<AttackEffectData> effects,
            DamageSource source,
            int chainDepth = 0)
        {
            this.attack = attack;
            this.baseDamage = baseDamage;
            this.modifiers = modifiers;
            this.effects = effects;
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
    }
}