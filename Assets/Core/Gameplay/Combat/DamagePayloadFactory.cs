using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.Modifiers;
using Core.Gameplay.Entity.Subsystem;
using Game.Entity.Player.Subsystem;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Gameplay.Combat
{
    public static class DamagePayloadFactory
    {
        public static DamagePayload Create(
            AttackInstance attack,
            int baseDamage,
            ModifierScope scope,
            DamageSource source)
        {
            if (attack == null)
                return default;

            // -----------------------------
            // 1. Collect Damage Modifiers
            // -----------------------------

            List<DamageModifier> filteredModifiers = null;
            
            var allModifiers  = source.sourceEntity
                ? source.sourceEntity
                    .GetComponent<EntityModifierSubsystem>()?
                    .DamageModifiers
                : null;
            
            if (allModifiers != null)
            {
                filteredModifiers = ListPool<DamageModifier>.Get();

                HitTypes actualHitTypes = attack.Data.hitTypes;

                //Debug.Log($"Total modifiers: {allModifiers.Count}");
                
                foreach (var mod in allModifiers)
                {
                    //Debug.Log($"Modifier: {mod.value} | HitType: {mod.hitTypeses}");
                    if (mod.AppliesTo(attack.Data, scope, actualHitTypes))
                        filteredModifiers.Add(mod);
                }
            }

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

            var payload = new DamagePayload(
                attack: attack,
                baseDamage: baseDamage,
                modifiers: filteredModifiers,
                effects: effects,
                source: source
            );

            return payload;
        }
        
        // --------------------------------
        // Chain Payload
        // --------------------------------

        public static DamagePayload CreateChain(
            DamagePayload previous,
            float multiplier,
            bool applyEffectsEveryBounce)
        {
            if (previous.attack == null)
                return default;

            IReadOnlyList<AttackEffectData> effects =
                applyEffectsEveryBounce
                    ? previous.effects
                    : null;

            return new DamagePayload(
                attack: previous.attack,
                baseDamage: Mathf.RoundToInt(previous.baseDamage * multiplier),
                modifiers: previous.modifiers,
                effects: effects,
                source: previous.source,
                chainDepth: previous.chainDepth + 1
            );
        }

        // --------------------------------
        // Effect Damage (DOT, Auras, etc.)
        // --------------------------------

        public static DamagePayload CreateEffectDamage(
            int damage,
            DamageSource source,
            List<DamageModifier> modifiers)
        {
            return new DamagePayload(
                attack: null,
                baseDamage: damage,
                modifiers: modifiers,
                effects: null,
                source: source,
                chainDepth: 0
            );
        }
    }
}
