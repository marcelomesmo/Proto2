using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity.Subsystem;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Gameplay.Combat
{
    public static class CombatPayloadFactory
    {
        public static CombatPayload Create(
            AttackInstance attack,
            int amount,
            CombatFlags flags,
            ModifierScope scope,
            AttackSource source)
        {
            if (attack == null)
                return default;

            // -----------------------------
            // 1. Merge Attack Effects
            // -----------------------------

            var effects = ListPool<StatusEffectData>.Get();

            // Base attack effects
            if (attack.Data.Effects != null)
                effects.AddRange(attack.Data.Effects);

            // Upgrade-provided effects
            var attackSubsystem =
                source.sourceEntity?
                    .GetComponent<EntityAttackSubsystem>();

            if (attackSubsystem != null)
            {
                var combined =
                    attackSubsystem.GetCombinedEffects(attack.Data);

                if (combined != null)
                {
                    effects.Clear();
                    effects.AddRange(combined);
                }
            }

            // -----------------------------
            // 2. Build Payload
            // -----------------------------

            var payload = new CombatPayload(
                action: attack.Data.combatActionType,
                attack: attack,
                amount: amount,
                effects: effects,
                source: source,
                flags: flags
            );

            return payload;
        }
        
        // --------------------------------
        // Chain Payload
        // --------------------------------

        public static CombatPayload CreateChain(
            CombatPayload previous,
            float multiplier,
            bool applyEffectsEveryBounce)
        {
            if (previous.attack == null)
                return default;

            IReadOnlyList<StatusEffectData> effects =
                applyEffectsEveryBounce
                    ? previous.effects
                    : null;

            return new CombatPayload(
                action: previous.attack.Data.combatActionType,
                attack: previous.attack,
                amount: Mathf.RoundToInt(previous.amount * multiplier),
                effects: effects,
                source: previous.source,
                flags: previous.flags,
                chainDepth: previous.chainDepth + 1
            );
        }

        // --------------------------------
        // Effect Damage (DOT, HOTs?, Auras, etc.)
        // --------------------------------

        public static CombatPayload CreateEffectDamage(
            int damage,
            AttackSource source)
        {
            return new CombatPayload(
                action: CombatAction.Damage,
                attack: null,
                amount: damage,
                effects: null,
                source: source
            );
        }
    }
}
