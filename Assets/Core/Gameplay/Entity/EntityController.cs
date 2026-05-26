using System;
using Core.Gameplay.Entity.Attack;
using Core.Gameplay.Entity.Spawn;
using Core.Gameplay.Entity.Stats;
using Core.Gameplay.Entity.Subsystem;
using Core.Gameplay.Entity.Tags;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Gameplay.Entity
{ 
    public abstract class EntityController : MonoBehaviour
    {
        public TagSystem Tags;
        protected BaseSubsystem[] Subsystems;
        public Animator Animator { get; private set; }
        public BaseEntityStats Stats { get; private set; }
        
        private bool _isConfigured;
        private bool _isSpawned;

        protected SpawnContext SpawnContext { get; private set; }
        
        public event Action<EntityController> DeathSignal;
        
        protected virtual void Awake()
        {
            Tags = new TagSystem();

            Subsystems = GetComponents<BaseSubsystem>();

            Animator = GetComponentInChildren<Animator>();  // VisualRoot child
        }
        
        public void Configure(in SpawnContext context)
        {
            Debug.Assert(!_isConfigured,
                $"[{name}] Configure called more than once.");

            Debug.Assert(context.Stats != null,
                $"[{name}] SpawnContext.StatsInstance is null.");

            // 1. Stats
            SpawnContext = context;
            Stats = Instantiate(context.Stats);

            // 2. Apply scaling / progression
            ApplySpawnScaling(context);

            // 3. Attack loadout
            if (context.AttackLoadout != null && TryGetComponent(out EntityAttackLoadout loadout))
                loadout.InitializeFromDefinition(context.AttackLoadout);
            
            _isConfigured = true;
        }
        protected virtual void ApplySpawnScaling(in SpawnContext context)
        {
            // Intentionally empty
        }
        
        public void InitializeAllSubsystems()
        {
            foreach (var s in Subsystems)
                s.Initialize(this);
        }

        public void DeinitializeAllSubsystems()
        {
            // Deactivate subsystems
            foreach (var s in Subsystems)
                s.Deinitialize();
        }
        
        private void OnEnable()
        {
            Tags.OnTagAdded += HandleTagAdded;
            Tags.OnTagRemoved += HandleTagRemoved;
        }

        private void OnDisable()
        {
            Tags.OnTagAdded -= HandleTagAdded;
            Tags.OnTagRemoved -= HandleTagRemoved;
        }

        protected virtual void HandleTagAdded(GameplayTag tag) { }
        protected virtual void HandleTagRemoved(GameplayTag tag) { }

        public abstract void NotifyDeathAnimationFinished();
        protected void RaiseDeathSignal()
        {
            DeathSignal?.Invoke(this);
        }

        #region Tag Checks
        
        public bool IsStunned => Tags.HasTag(Stats.stunTag);
        public bool IsSlowed => Tags.HasTag(Stats.slowTag);
        public bool IsBurning => Tags.HasTag(Stats.burnTag);
        //public bool IsFrozen => ;
        //public bool IsEnraged => ;
        // Resilient, accessor must be defensive since it's queried from other entities. Correct approach for pooled entities.
        public bool IsDead
        {
            get
            {
                if (this == null) return true;
                if (!gameObject) return true;
                if (Stats == null) return true;
                if (Tags == null) return true;

                return Tags.HasTag(Stats.deadTag);
            }
        }
        public bool IsInvulnerable => Tags.HasTag(Stats.invulnerableTag);
       
        #endregion
        
        #region Pool
        
        private IObjectPool<EntityController> _objectPool;
        public bool IsReleased { get; private set; }
        public void AssignToPool(IObjectPool<EntityController> objectPool) => _objectPool = objectPool;

        protected void ReturnToPool()
        {
            Debug.Assert(!IsReleased,
                $"[{name}] Attempted to release entity twice.");
            
            IsReleased = true;
            _objectPool.Release(this);
        }
        public bool TryGetAssignedPool(out IObjectPool<EntityController> pool) { pool = _objectPool; return pool != null; }
        
        #endregion
    
        #region Pool lifecycle helpers
        
        // Called by pool on Get (actionOnGet)
        public void OnSpawn()
        {
            Debug.Assert(_isConfigured,
                $"[{name}] OnSpawn called before Configure.");

            Debug.Assert(!_isSpawned,
                $"[{name}] OnSpawn called more than once.");
            
            IsReleased = false;
            
            if(!Stats)
                throw new System.Exception("[EntityController] Stats not set!");

            InitializeAllSubsystems();
            
            // Rebind ONLY here
            Animator.Rebind();
            Animator.Update(0f);
            
            _isSpawned = true;
        }

        // Called by pool on Release (actionOnRelease)
        public void OnDespawn()
        {
            Debug.Assert(_isSpawned,
                $"[{name}] OnDespawn called without OnSpawn.");
            
            DeinitializeAllSubsystems();
            
            Tags.ClearTags();
            
            ResetState();
        }
        
        private void ResetState()
        {
            DeathSignal = null;
            
            _isConfigured = false;
            _isSpawned = false;
            SpawnContext = default;
            Stats = null;
        }

        #endregion
    }
}
