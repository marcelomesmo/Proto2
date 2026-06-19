using Core.Enum;
using Core.Gameplay.Entity.Stats;
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
        
        public float GetResolvedChainAmountMultiplier()
        {
            if (!Data.chainData)
                return 0;

            return AttackStatResolver.Resolve(
                baseValue: Data.chainData.damageMultiplierPerBounce,
                attack: Data,
                statType: AttackStatType.ChainDamageMultiplier,
                scope: ResolveScope(),
                hitTypes: Data.hitTypes,
                modifiers: _modifiers.AttackStatModifiers
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
                AttackExecutionMode.Area => ModifierScope.Area,
                AttackExecutionMode.Direct => ModifierScope.Direct,
                _ => ModifierScope.All
            };
        }
        
        public int GetResolvedCombatAmount(
            AttackResolveContext context)
        {
            float value = 0f;

            switch(Data.combatActionType)
            {
                case CombatAction.Damage:
                    // Apply modifiers
                    value = 
                        AttackStatResolver.Resolve(
                            baseValue: Data.amount,
                            attack: Data,
                            statType: AttackStatType.Damage,
                            scope: ResolveScope(),
                            hitTypes: Data.hitTypes,
                            modifiers: _modifiers.AttackStatModifiers
                        );

                    // Apply stat bonuses
                    value += GetDamageBonus(context);
                    break;

                case CombatAction.Heal:
                    value =  
                        AttackStatResolver.Resolve(
                            baseValue: Data.amount,
                            attack: Data,
                            statType: AttackStatType.Healing,
                            scope: ResolveScope(),
                            hitTypes: Data.hitTypes,
                            modifiers: _modifiers.AttackStatModifiers
                        );
                    
                    value += GetHealingBonus(context);
                    break;
            }

            // Future:
            // crit bonus
            // berserk
            // temporary buffs
            // aura modifiers
            // debuffs
            // difficulty scaling
            // etc
            
            // Simple for now, but later we can do:
            //  stats.attackPower + _temporaryAttackBuff, or
            //  stats.attackPower * (IsEnraged ? 2 : 1); or
            //  stats.attackPower * stats.attackPowerMultiplier; etc.
            
            return Mathf.RoundToInt(value);
        }
        
        private int GetDamageBonus(AttackResolveContext context)
        {
            if (context.Source.Stats)
                return Mathf.RoundToInt(context.Source.Stats.attackPower);
            
            return 0;
        }
        
        private int GetHealingBonus(AttackResolveContext context)
        {
            if (context.Source.Stats)
                return Mathf.RoundToInt(context.Source.Stats.healingPower);

            return 0;
        }
    }
}
