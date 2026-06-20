using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.Modifiers;
using UnityEngine;

namespace Core.Gameplay.Combat
{
    public static class AttackStatResolver
    {
        public static float Resolve(
            float baseValue,
            AttackData attack,
            AttackStatType statType,
            ModifierScope scope,
            HitTypes hitTypes,
            IReadOnlyList<AttackStatModifier> modifiers)
        {
            float value = baseValue;

            if (modifiers == null || modifiers.Count == 0)
                return value;
            
            // 1. Resolve in steps: this is important to guarantee the result is deterministic (additive first, then multiplicative).
            //      - all additive modifiers resolve first
            //      - all multiplicative modifiers resolve second

            // -------------------------
            // Additive
            // -------------------------
            foreach (var mod in modifiers)
            {
                if (!mod.AppliesTo(attack, statType, scope, hitTypes))
                    continue;

                if (mod.type != ModifierType.Additive)
                    continue;

                value += mod.value;
            }

            // -------------------------
            // Multiplicative
            // -------------------------
            foreach (var mod in modifiers)
            {
                if (!mod.AppliesTo(attack, statType, scope, hitTypes))
                    continue;

                if (mod.type != ModifierType.Multiplicative)
                    continue;

                value *= mod.value;
            }

            return Mathf.Max(0f, value);
        }
    }
}
