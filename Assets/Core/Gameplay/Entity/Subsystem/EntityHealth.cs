using System;
using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.ChainAttack;
using Core.Gameplay.Combat.Modifiers;
using Core.Gameplay.Combat.StatusEffect;
using Core.Gameplay.Combat.StatusEffect.Implementations;
using Core.Gameplay.Entity.Tags;
using Core.Interfaces;
using Game.Entity.Player.Subsystem;
using UnityEngine;

namespace Core.Gameplay.Entity.Subsystem
{
    public class EntityHealth : BaseSubsystem, IDamageable
    {
        public Faction Faction => Controller.Stats.faction;
        
        private int _currentHealth;
        private int _maxHealth;
        public int GetCurrentHealth() => _currentHealth;
        public int GetCurrentMaxHealth() => _maxHealth;
        
        public event Action<int, int> HealthChanged;
        public event Action<DamagePayload> OnDamageTaken;
        
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
        
        public void TakeDamage(DamagePayload payload)
        {
            // Extra defensive check — in case someone calls TakeDamage directly:
            if (!CanBeDamaged()) return;
            
            // 1. Apply damage
            int damage = payload.ResolveDamage();
            
            _currentHealth = Mathf.Clamp(
                _currentHealth - damage,
                0,
                _maxHealth
            );
            
            // 2. Apply status effects
            ApplyAttackEffects(payload.effects, payload.source);
            
            // 3. Play animation
            //Controller.Animator.SetTrigger("hurt");

            // 4. Raise events
            //if (Faction == Faction.Player) OnHealthChanged.RaiseEvent(_currentHealth, Controller.Stats.maxHealth);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnDamageTaken?.Invoke(payload);
            
            // 5. Resolve Chain Attacks: this coupling is intentional (for now).
            if (payload.hitData != null &&
                payload.hitData.chainData != null &&
                payload.chainDepth == 0)
            {
                ChainAttackResolver.ResolveChain(
                    payload.hitData,
                    payload,
                    this
                );
            }
            
            // 6. Finally, check status.
            if (_currentHealth <= 0)
                Die();
        }

        public void ApplyAttackEffects(IReadOnlyList<AttackEffectData> effects, DamageSource source)
        {
            if (effects == null || effects.Count == 0)
                return;

            if (!TryGetComponent(out EntityStatusEffectSubsystem entityStatusEffectSubsystem))
                return;
            
            foreach (var effect in effects)
            {
                StatusEffectInstance instance = effect.effectType switch
                {
                    StatusEffectType.Burn =>
                        new BurnEffectInstance(
                            Controller, 
                            effect, 
                            source,
                            Controller.Stats.burnTag),

                    StatusEffectType.Stun =>
                        new StunEffectInstance(
                            Controller,
                            effect,
                            Controller.Stats.stunTag),

                    StatusEffectType.Slow =>
                        new SlowEffectInstance(
                            Controller,
                            effect,
                            Controller.Stats.slowTag),

                    _ => null
                };

                if (instance == null) return;
                
                entityStatusEffectSubsystem.AddEffect(instance);
            }
        }

        public void Heal(int healing)
        {
            //_currentHealth = Mathf.Clamp(_currentHealth + healing, 0, Controller.Stats.maxHealth);

            //if (Faction == Faction.Player)
            //    OnHealthChanged.RaiseEvent(_currentHealth, Controller.Stats.maxHealth);
            //else 
            //    HealthChanged?.Invoke(_currentHealth, Controller.Stats.maxHealth);
            
            //PlayVFX_OnHeal();
        }

        // Keep CanBeDamaged() extremely cheap (only boolean checks).
        // Heavy checks (animations, raycasts, slow queries) should be avoided there.
        public bool CanBeDamaged()
        {
            return !Controller.Tags.HasTag(Controller.Stats.deadTag)
                   && !Controller.Tags.HasTag(Controller.Stats.invulnerableTag);
        }

        private void Die()
        {
            Controller.Tags.AddTag(Controller.Stats.deadTag);
        }
        
        public bool IsDead => _currentHealth <= 0;
    }
}