using System;
using Core.Enum;
using Core.Gameplay.Combat;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity.Tags;
using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Entity.Subsystem
{
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
        
        protected override void OnInitialize()
        {
            ResetHealth();
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

            var modifiers =
                Controller.GetComponent<EntityModifierSubsystem>()
                    ?.HealthModifiers;

            if (modifiers == null)
                return result;

            foreach (var mod in modifiers)
            {
                Debug.Log("[EntityHealth] Found modifier value: " + mod.value);
                
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

        private void ApplyDamage(CombatPayload payload)
        {
            // Extra defensive check — in case someone calls TakeDamage directly:
            if (!CanReceiveCombat(payload)) 
                return;
            
            // 1. Apply damage
            int damage = payload.ResolveAmount();
            
            _currentHealth = Mathf.Clamp(
                _currentHealth - damage,
                0,
                _maxHealth
            );

            // 2. Raise events
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnDamageTaken?.Invoke(payload);

            // 3. Apply Effects
            ApplyCombatEffects(payload);
            
            // 4. Finally, check status.
            if (_currentHealth <= 0)
                Die();
        }

        private void ApplyHeal(CombatPayload payload)
        {
            if (!CanReceiveCombat(payload))
                return;

            _currentHealth = Mathf.Clamp(
                _currentHealth + payload.ResolveAmount(),   // TODO: how we filter healing modifiers?
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

            if (!Controller.TryGetComponent<EntityStatusEffectSubsystem>(
                    out var statusSubsystem))
                return;

            foreach (var effect in payload.effects)
            {
                var instance =
                    StatusEffectFactory.Create(
                        effect,
                        Controller,
                        payload.source);

                if (instance != null)
                    statusSubsystem.AddEffect(instance);
            }
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