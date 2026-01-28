using System;
using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.ChainAttack;
using Core.Gameplay.Combat.StatusEffect;
using Core.Gameplay.Combat.StatusEffect.Implementations;
using Core.Gameplay.Entity.Tags;
using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Entity.Subsystem
{
    public class EntityHealth : BaseSubsystem, IDamageable
    {
        public Faction Faction => Controller.Stats.faction;
        
        private int _currentHealth;
        public int GetCurrentHealth() => _currentHealth;
        
        //[Header("Broadcast (Player only)")]
        //[SerializeField] protected IntIntEventChannelSO OnHealthChanged;
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
        // Tag Reactions
        // ---------------------------
        protected override void HandleTagAdded(GameplayTag tag)
        {
        }

        protected override void HandleTagRemoved(GameplayTag tag)
        {
        }

        // ---------------------------
        // Health Logic
        // ---------------------------
        
        public void ResetHealth()
        {
            _currentHealth = Controller.Stats.maxHealth;
            
            // Initialize Health in HUD
            //if (Faction == Faction.Player)
            //    OnHealthChanged?.RaiseEvent(_currentHealth, Controller.Stats.maxHealth);
            //else
                HealthChanged?.Invoke(_currentHealth, Controller.Stats.maxHealth);
        }

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
                Controller.Stats.maxHealth
            );
            
            // 2. Apply status effects
            ApplyAttackEffects(payload.effects, payload.source);
            
            // 3. Play animation
            //Controller.Animator.SetTrigger("hurt");

            // 4. Raise events
            //if (Faction == Faction.Player) OnHealthChanged.RaiseEvent(_currentHealth, Controller.Stats.maxHealth);
            HealthChanged?.Invoke(_currentHealth, Controller.Stats.maxHealth);
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
            Controller.Tags.AddTag(Controller.Stats.deadTag);   // todo: vfx subsystem on death tag added play audio?
            Controller.Animator.SetBool("isDead", true);
            Controller.Animator.ResetTrigger("attack");
            Controller.Animator.ResetTrigger("jump");
            Controller.Animator.ResetTrigger("dash");
            Controller.Animator.ResetTrigger("spawned");
            
            //if(deathAudio)
            //    EntityAudio.Play(deathAudio);
        }
        
        // ---------------------------
        // Animation Events
        // ---------------------------
        // Called via animation event at end of Death animation
        public void NotifyDeathAnimationFinished()
        {
            Controller.NotifyDeathAnimationFinished();
        }
       
        public void NotifySpawnAnimationFinished()
        {
            Controller.Animator.SetTrigger("spawned");
            
            Controller.Tags.RemoveTag(Controller.Stats.invulnerableTag); // Force loss of invulnerability
            Controller.Tags.AddTag(Controller.Stats.spawnFinishedTag);
        }

        public bool IsDead => _currentHealth <= 0;
    }
}