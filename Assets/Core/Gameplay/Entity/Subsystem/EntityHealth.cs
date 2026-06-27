using System;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity.Tags;
using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Entity.Subsystem
{
    [RequireComponent(typeof(EntityModifierSubsystem))]
    public class EntityHealth : BaseSubsystem, ICombatReceiver
    {
        public Faction Faction => Controller.Stats.faction;
        
        private int _currentHealth;
        private int _maxHealth;
        public int GetCurrentHealth() => _currentHealth;
        public int GetCurrentMaxHealth() => _maxHealth;
        
        public event Action<int, int> HealthChanged;
        public event Action<CombatPayload> OnDamageTaken;
        public event Action<CombatPayload> OnHealingTaken;
        
        // Helpers - cached
        private EntityModifierSubsystem _modifiers;
        
        protected override void OnInitialize()
        {
            ResetHealth();
            
            _modifiers = GetComponent<EntityModifierSubsystem>();
        }
        
        protected override void OnDeinitialize()
        {
            // Reset state for pooling
            _currentHealth = 0;
        }
        
        // ---------------------------
        // Health Initialize / Adjust
        // ---------------------------
        public void ResetHealth()
        {
            _maxHealth = ResolveMaxHealth();

            _currentHealth = _maxHealth;
            
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
        }
        
        private int ResolveMaxHealth()
        {
            int result = Controller.Stats.maxHealth;

            var modifiers = _modifiers?.HealthModifiers;

            if (modifiers == null)
                return result;

            foreach (var mod in modifiers)
            {
                switch (mod.type)
                {
                    case ModifierType.Additive:
                        result += Mathf.RoundToInt(mod.value);
                        break;

                    case ModifierType.Multiplicative:
                        result = Mathf.RoundToInt(result * mod.value);
                        break;
                }
            }

            return Mathf.Max(1, result);
        }
        
        // ---------------------------
        // Tag Reactions
        // ---------------------------
        protected override void HandleTagAdded(GameplayTag tag)
        {
        }

        protected override void HandleTagRemoved(GameplayTag tag)
        {
        }

        // ---------------------------
        // Health Change Logic
        // ---------------------------

        public void SetInvulnerable(bool invulnerable)
        {
            if(invulnerable) Controller.Tags.AddTag(Controller.Stats.invulnerableTag);
            else Controller.Tags.RemoveTag(Controller.Stats.invulnerableTag);
        }

        public void ReceiveDamage(CombatPayload payload)
        {
            ApplyDamage(payload);
        }

        public void ReceiveHeal(CombatPayload payload)
        {
            ApplyHeal(payload);
        }
        
        public void ReceiveStatusEffect(CombatPayload payload)
        {
            ApplyCombatEffects(payload);
        }

        private void ApplyDamage(CombatPayload payload)
        {
            // Extra defensive check — in case someone calls TakeDamage directly:
            if (!CanReceiveCombat(payload)) 
                return;

            var resolvedDefense = GetResolvedDefense(payload.attack);
            var resolvedResistance = GetResolvedResistance(payload.attack);
            
            // 1. Apply damage
            int postDefense = Mathf.Max(0, payload.amount - resolvedDefense.amount);
            int mitigatedDamage = Mathf.RoundToInt(postDefense * (1f - resolvedResistance.amount));
            
            /*if(Faction == Faction.Player)
                Debug.Log("[Castle] Total damage = (dmg:" + payload.amount + 
                          "[" + payload.attack.Data.hitTypes.ToString() + "] - def:" + resolvedDefense.amount + 
                          ") * (1 - res:" + resolvedResistance.amount + 
                          ") = " + mitigatedDamage);*/
            
            // Rebuild payload with merged flags and post-mitigation amount
            // so listeners on OnDamageTaken see the final resolved state.
            CombatPayload resolvedPayload = new CombatPayload(
                action: payload.action,
                attack: payload.attack,
                amount: mitigatedDamage,
                effects: payload.effects,
                source: payload.source,
                flags: payload.flags | resolvedDefense.flags | resolvedResistance.flags,
                chainDepth: payload.chainDepth
            );
            
            _currentHealth = Mathf.Clamp(
                _currentHealth - mitigatedDamage,
                0,
                _maxHealth
            );

            // 2. Raise events
            HealthChanged?.Invoke(_currentHealth, _maxHealth);  // TODO: Pass flags here in case we want VFX feedback in the HealthBar
            OnDamageTaken?.Invoke(resolvedPayload);

            // 3. Apply Effects
            ApplyCombatEffects(resolvedPayload);
            
            // 4. Finally, check status.
            if (_currentHealth <= 0)
                Die();
        }

        private void ApplyHeal(CombatPayload payload)
        {
            if (!CanReceiveCombat(payload))
                return;

            _currentHealth = Mathf.Clamp(
                _currentHealth + payload.amount,
                0,
                _maxHealth);

            HealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnHealingTaken?.Invoke(payload);

            ApplyCombatEffects(payload);
        }
        
        private void ApplyCombatEffects(CombatPayload payload)
        {
            
            if (payload.effects == null ||
                payload.effects.Count == 0)
                return;

            // TODO: make this base in EntityHealth instead
            if (!Controller.TryGetComponent<EntityStatusEffectSubsystem>(
                    out var statusSubsystem))
                return;
            
            statusSubsystem.ApplyEffectsFromPayload(payload);
        }
        
        private CombatAmountResult<int> GetResolvedDefense(AttackInstance attackInstance)
        {
            if (attackInstance == null) // Effect's have null attack instances when applied.
                return new CombatAmountResult<int>(
                    amount: Mathf.RoundToInt(Controller.Stats.defense),
                    flags: CombatFlags.None
                );
            
            AttackResolveContext context = 
                new AttackResolveContext 
                {
                    Source = this.Controller,
                    Modifiers = _modifiers
                };

            return attackInstance.GetResolvedDefenseAmount(context);
        }
        
        private CombatAmountResult<float> GetResolvedResistance(AttackInstance attackInstance)
        {
            if (attackInstance == null) // Effect's have null attack instances when applied.
                return new CombatAmountResult<float>(
                    amount: Mathf.RoundToInt(Controller.Stats.resistance),
                    flags: CombatFlags.None
                );
            
            AttackResolveContext context = 
                new AttackResolveContext 
                {
                    Source = this.Controller,
                    Modifiers = _modifiers
                };

            return attackInstance.GetResolvedResistanceAmount(context);
        }

        // Keep CanBeDamaged() extremely cheap (only boolean checks).
        // Heavy checks (animations, raycasts, slow queries) should be avoided there.
        public bool CanReceiveCombat(CombatPayload payload)
        {
            if (Controller.IsDead &&
                payload.action != CombatAction.Revive)
                return false;
            
            if(Controller.IsInvulnerable &&
               payload.action == CombatAction.Damage)
                return false;
            
            return payload.source.targetFilter.CanTarget(
                payload.source,
                Controller
            );
        }

        private void Die()
        {
            Controller.Tags.AddTag(Controller.Stats.deadTag);
        }
        
        public bool IsDead => _currentHealth <= 0;
    }
}