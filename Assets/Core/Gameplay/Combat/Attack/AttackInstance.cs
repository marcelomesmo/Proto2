using Core.Enum;
using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public readonly struct CombatAmountResult<T>
    {
        public readonly T amount;
        public readonly CombatFlags flags;

        public CombatAmountResult(T amount, CombatFlags flags)
        {
            this.amount = amount;
            this.flags = flags;
        }
    }
    
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
        
        public CombatAmountResult<int> GetResolvedCombatAmount(
            AttackResolveContext context)
        {
            float value = 0f;
            var resolvedScope = ResolveScope();

            //
            //  1. Calculate base amount and stat modifiers
            //
            switch(Data.combatActionType)
            {
                case CombatAction.Damage:
                    // Apply modifiers
                    value = 
                        AttackStatResolver.Resolve(
                            baseValue: Data.amount,
                            attack: Data,
                            statType: AttackStatType.Damage,
                            scope: resolvedScope,
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
                            scope: resolvedScope,
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
            
            //
            //  2. Crit step
            //
            CombatFlags flags = CombatFlags.None;
            
            if (RollCrit(context, resolvedScope))
            {
                flags |= CombatFlags.Critical;
                float critDamageBonus = ResolveCritDamage(context, resolvedScope);
                value = value + (critDamageBonus * value);
            }

            
            //
            //  n. Return final value with resulting flags
            //
            return new CombatAmountResult<int>(
                amount: Mathf.RoundToInt(value),
                flags: flags
            );
            
            // Simple for now, but later we can do:
            //  stats.attackPower + _temporaryAttackBuff, or
            //  stats.attackPower * (IsEnraged ? 2 : 1); or
            //  stats.attackPower * stats.attackPowerMultiplier; etc.
            
            //return Mathf.RoundToInt(value);
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
        
        //
        //  Critical resolution
        //
        private bool RollCrit(AttackResolveContext context, ModifierScope scope)
        {
            float critChance = 0f;

            if (context.Source != null && context.Source.Stats != null)
                critChance = context.Source.Stats.critChance;

            critChance = AttackStatResolver.Resolve(
                baseValue: critChance,
                attack: Data,
                statType: AttackStatType.CritChance,
                scope: scope,
                hitTypes: Data.hitTypes,
                modifiers: _modifiers.AttackStatModifiers
            ) / 100f;   // divided to keep input 0-100

            return UnityEngine.Random.value < critChance;
        }

        // TODO: Later update this to also use CombatAmountResult<float> and handle flag addition here.
        //      Remove flag addition from the call in EntityAttackSubsystem (trace back to find).
        private float ResolveCritDamage(AttackResolveContext context, ModifierScope scope)
        {
            float critDamage = 0f;

            if (context.Source != null && context.Source.Stats != null)
                critDamage = context.Source.Stats.critDamage;

            return AttackStatResolver.Resolve(
                baseValue: critDamage,
                attack: Data,
                statType: AttackStatType.CritDamage,
                scope: scope,
                hitTypes: Data.hitTypes,
                modifiers: _modifiers.AttackStatModifiers
            ) / 100f;   // divided to keep input 0-100
        }
        
        //
        //  Defense and Health
        //
        public CombatAmountResult<int> GetResolvedDefenseAmount(AttackResolveContext context)
        {
            float baseDefense = context.Source.Stats.defense;
            var resolvedScope = ResolveScope();

            // 1. Base Defense value
            float resolved = AttackStatResolver.Resolve(
                baseValue: baseDefense,
                attack: Data,
                statType: AttackStatType.Defense,
                scope: resolvedScope,
                hitTypes: Data.hitTypes,
                modifiers: context.Modifiers.AttackStatModifiers
            );
            
            // 2. Any other application that might result in flag: Resisted, Blocked, etc.
            CombatFlags flags = CombatFlags.None;
            
            
            // n. Return final value with resulting flags
            return new CombatAmountResult<int>(
                amount: Mathf.RoundToInt(resolved),
                flags: flags
            );
        }
        
        public CombatAmountResult<float> GetResolvedResistanceAmount(AttackResolveContext context)
        {
            float baseResistance = context.Source.Stats.resistance;
            var resolvedScope = ResolveScope();

            float resolved = AttackStatResolver.Resolve(
                baseValue: baseResistance,
                attack: Data,
                statType: AttackStatType.Resistance,
                scope: resolvedScope,
                hitTypes: Data.hitTypes,
                modifiers: context.Modifiers.AttackStatModifiers
            ) / 100f;   // divided to keep input 0-100
            
            CombatFlags flags = CombatFlags.None;
            if (resolved > 0f)
                flags |= CombatFlags.Resisted;

            return new CombatAmountResult<float>(resolved, flags);
        }
    }
}
