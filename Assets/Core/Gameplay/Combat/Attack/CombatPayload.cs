using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Modifiers;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public readonly struct CombatPayload
    {
        // What happened
        public readonly CombatAction action;
        public readonly AttackInstance attack;                             // null for DOT / effect damage
        public readonly int amount;                                 // ALWAYS set
        public readonly IReadOnlyList<DamageModifier> modifiers;
        public readonly IReadOnlyList<AttackEffectData> effects;        // null for DOT / effect damage
        public readonly AttackSource source;
        public readonly int chainDepth;
        
        public CombatPayload(
            CombatAction action,
            AttackInstance attack,
            int amount,
            IReadOnlyList<DamageModifier> modifiers,
            IReadOnlyList<AttackEffectData> effects,
            AttackSource source,
            int chainDepth = 0)
        {
            this.action = action;
            this.attack = attack;
            this.amount = amount;
            this.modifiers = modifiers;
            this.effects = effects;
            this.source = source;
            this.chainDepth = chainDepth;
        }
        
        public int ResolveAmount()
        {
            float value = amount;

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