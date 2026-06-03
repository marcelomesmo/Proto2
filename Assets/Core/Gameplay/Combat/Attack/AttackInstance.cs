using Core.Enum;
using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public sealed class AttackInstance
    {
        private readonly EntityModifierSubsystem _modifiers;
        public AttackData Data { get; }
        public float CooldownRemaining { get; private set; }
        
        private int _castCount;
        public int CastCount => _castCount;

        public AttackInstance(AttackData data, EntityModifierSubsystem modifiers)
        {
            Data = data;
            _modifiers = modifiers;
            CooldownRemaining = 0f;
        }

        public bool IsReady => CooldownRemaining <= 0f;

        public void Tick(float deltaTime)
        {
            if (CooldownRemaining > 0f)
                CooldownRemaining -= deltaTime;
        }

        public void Consume()
        {
            _castCount++;
            CooldownRemaining = GetCooldown();
        }
        
        public float GetCooldown()
        {
            return AttackStatResolver.Resolve(
                baseValue: Data.cooldown,
                attack: Data,
                statType: AttackStatType.Cooldown,
                scope: ResolveScope(),
                hitTypes: Data.hitTypes,
                modifiers: _modifiers.AttackStatModifiers
            );
        }
        
        public float GetRange()
        {
            return AttackStatResolver.Resolve(
                baseValue: Data.range,
                attack: Data,
                statType: AttackStatType.Range,
                scope: ResolveScope(),
                hitTypes: Data.hitTypes,
                modifiers: _modifiers.AttackStatModifiers
            );
        }

        public float GetDuration()
        {
            if (!Data.areaEffectData)
                return 0f;

            return AttackStatResolver.Resolve(
                baseValue: Data.areaEffectData.duration,
                attack: Data,
                statType: AttackStatType.Duration,
                scope: ResolveScope(),
                hitTypes: Data.hitTypes,
                modifiers: _modifiers.AttackStatModifiers
            );
        }
        
        public int GetResolvedChainBounces()
        {
            if (!Data.chainData)
                return 0;

            return Mathf.RoundToInt(
                AttackStatResolver.Resolve(
                baseValue: Data.chainData.maxBounces,
                attack: Data,
                statType: AttackStatType.ExtraChainBounces,
                scope: ResolveScope(),
                hitTypes: Data.hitTypes,
                modifiers: _modifiers.AttackStatModifiers
                )
            );
        }
        
        public int GetResolvedChainDamageMultiplier()
        {
            if (!Data.chainData)
                return 0;

            return Mathf.RoundToInt(
                AttackStatResolver.Resolve(
                    baseValue: Data.chainData.damageMultiplierPerBounce,
                    attack: Data,
                    statType: AttackStatType.ChainDamageMultiplier,
                    scope: ResolveScope(),
                    hitTypes: Data.hitTypes,
                    modifiers: _modifiers.AttackStatModifiers
                )
            );
        }
        
        public int GetResolvedExtraExecutions()
        {
            return Mathf.RoundToInt(
                AttackStatResolver.Resolve(
                    baseValue: 0,
                    attack: Data,
                    statType: AttackStatType.ExtraExecutions,
                    scope: ResolveScope(),
                    hitTypes: Data.hitTypes,
                    modifiers: _modifiers.AttackStatModifiers
                )
            );
        }
        
        public AttackInstance GetResolvedAttackVariant(
            AttackContext context,
            EntityAttackSubsystem attackSubsystem)
        {
            return AttackVariantResolver.Resolve(
                this,
                context,
                _modifiers.AttackVariantModifiers,
                attackSubsystem);
        }

        private ModifierScope ResolveScope()
        {
            return Data.executionMode switch
            {
                AttackExecutionMode.Melee => ModifierScope.Melee,
                AttackExecutionMode.Projectile => ModifierScope.Projectile,
                AttackExecutionMode.AreaEffect => ModifierScope.Area,
                _ => ModifierScope.All
            };
        }
    }
}
