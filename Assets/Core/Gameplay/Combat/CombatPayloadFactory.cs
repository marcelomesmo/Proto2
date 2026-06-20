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
            ModifierScope scope,
            AttackSource source)
        {
            if (attack == null)
                return default;

            // -----------------------------
            // 1. Collect Damage Modifiers
            // -----------------------------

            /*
             
             DEPRECATED, kept here just in case.
             
             List<AttackStatModifier> filteredModifiers = null;
            
            var allModifiers  = source.sourceEntity
                ? source.sourceEntity
                    .GetComponent<EntityModifierSubsystem>()?
                    .AttackStatModifiers
                : null;
            
            if (allModifiers != null)
            {
                filteredModifiers = ListPool<AttackStatModifier>.Get(); // TODO: When is this release? Do we need to?

                HitTypes actualHitTypes = attack.Data.hitTypes;

                //Debug.Log($"Total modifiers: {allModifiers.Count}");
                
                foreach (var mod in allModifiers)
                {
                    var type = attack.Data.combatActionType == CombatAction.Damage
                        ? AttackStatType.Damage
                        : AttackStatType.Healing;
                    
                    //Debug.Log($"Modifier: {mod.value} | HitType: {mod.hitTypeses}");
                    if (mod.AppliesTo(attack.Data, type, scope, actualHitTypes))
                        filteredModifiers.Add(mod);
                }
            }
            */

            // -----------------------------
            // 2. Merge Attack Effects
            // -----------------------------

            var effects = ListPool<AttackEffectData>.Get();

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
            // 3. Build Payload
            // -----------------------------

            var payload = new CombatPayload(
                action: attack.Data.combatActionType,
                attack: attack,
                amount: amount,
                effects: effects,
                source: source
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

            IReadOnlyList<AttackEffectData> effects =
                applyEffectsEveryBounce
                    ? previous.effects
                    : null;

            return new CombatPayload(
                action: previous.attack.Data.combatActionType,
                attack: previous.attack,
                amount: Mathf.RoundToInt(previous.amount * multiplier),
                effects: effects,
                source: previous.source,
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
                source: source,
                chainDepth: 0
            );
        }
    }
}
