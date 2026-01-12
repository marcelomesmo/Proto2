using System;
using Core.Audio.Data;
using Core.Enum;
using Core.EventChannels;
using Core.Gameplay.Combat.Attack;
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
        
        [Header("Broadcast (Player only)")]
        [SerializeField] protected IntIntEventChannelSO OnHealthChanged;
        public event Action<int, int> HealthChanged;
        public event Action<DamagePayload> DamageTaken;
        
        [Header("Audio")]
        [SerializeField] private AudioEvent deathAudio;
        
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
            if (Faction == Faction.Player)
                OnHealthChanged.RaiseEvent(_currentHealth, Controller.Stats.maxHealth);
            else
                HealthChanged?.Invoke(_currentHealth, Controller.Stats.maxHealth);
        }

        public void SetInvulnerable(bool invulnerable)
        {
            if(invulnerable) Controller.Tags.AddTag(Controller.Stats.invulnerableTag);
            else Controller.Tags.RemoveTag(Controller.Stats.invulnerableTag);
        }
        
        // FUTURE IMPROVEMENTS
        /*
            Create a damage info payload:
            
            public struct DamageInfo
            {
                public int amount;
                public Vector2 hitPoint;
                public Vector2 direction;
                public DamageType type;
                public bool isCritical;
                public GameObject source;
            }

            public void TakeDamage(DamageInfo info) { ... }
            
         */
        
        public void TakeDamage(DamagePayload payload)
        {
            // Extra defensive check — in case someone calls TakeDamage directly:
            if (!CanBeDamaged()) return;
            
            _currentHealth = Mathf.Clamp(_currentHealth - payload.hitData.damage, 0, Controller.Stats.maxHealth);
            //Controller.Animator.SetTrigger("hurt");

            if (Faction == Faction.Player)
                OnHealthChanged.RaiseEvent(_currentHealth, Controller.Stats.maxHealth);
            
            HealthChanged?.Invoke(_currentHealth, Controller.Stats.maxHealth);
            DamageTaken?.Invoke(payload);
            
            if (_currentHealth <= 0)
                Die();
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
            Controller.Animator.SetBool("isDead", true);
            Controller.Animator.ResetTrigger("attack");
            Controller.Animator.ResetTrigger("jump");
            Controller.Animator.ResetTrigger("dash");
            Controller.Animator.ResetTrigger("spawned");
            
            if(deathAudio)
                EntityAudio.Play(deathAudio);
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