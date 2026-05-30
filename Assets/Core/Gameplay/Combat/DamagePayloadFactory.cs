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
            AttackData attackData,
            int baseDamage,
            ModifierScope scope,
            DamageSource source)
        {
            if (attackData == null)
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

                HitTypes actualHitTypes = attackData.hitTypes;

                //Debug.Log($"Total modifiers: {allModifiers.Count}");
                
                foreach (var mod in allModifiers)
                {
                    //Debug.Log($"Modifier: {mod.value} | HitType: {mod.hitTypeses}");
                    if (mod.AppliesTo(attackData, scope, actualHitTypes))
                        filteredModifiers.Add(mod);
                }
            }

            // -----------------------------
            // 2. Merge Attack Effects
            // -----------------------------

            var effects = ListPool<AttackEffectData>.Get();

            // Base attack effects
            if (attackData.Effects != null)
                effects.AddRange(attackData.Effects);

            // Upgrade-provided effects
            var attackSubsystem =
                source.sourceEntity?
                    .GetComponent<EntityAttackSubsystem>();

            if (attackSubsystem != null)
            {
                var combined =
                    attackSubsystem.GetCombinedEffects(attackData);

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
                hitData: attackData,
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
            AttackData attackData,
            float multiplier,
            bool applyEffectsEveryBounce)
        {
            if (previous.hitData == null)
                return default;

            IReadOnlyList<AttackEffectData> effects =
                applyEffectsEveryBounce
                    ? previous.effects
                    : null;

            return new DamagePayload(
                hitData: previous.hitData,
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
                hitData: null,
                baseDamage: damage,
                modifiers: modifiers,
                effects: null,
                source: source,
                chainDepth: 0
            );
        }
    }
}
