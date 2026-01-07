using Core.Gameplay.Entity.Stats;
using Core.Gameplay.Entity.Subsystem;
using Core.Gameplay.Entity.Tags;
using Enemy;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Gameplay.Entity
{ 
    public abstract class EntityController : MonoBehaviour
    {
        public TagSystem Tags;
        protected BaseSubsystem[] Subsystems;
        public Animator Animator;
        public BaseEntityStats Stats;

        protected virtual void Awake()
        {
            Tags = new TagSystem();

            Subsystems = GetComponents<BaseSubsystem>();

            Animator = GetComponentInChildren<Animator>();  // VisualRoot child
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

        #region Tag Checks
        public bool IsStunned => Tags.HasTag(Stats.stunnedTag);
        //public bool IsFrozen => ;
        //public bool IsEnraged => ;
        public bool IsDead => Tags.HasTag(Stats.deadTag);
        public bool IsInvulnerable => Tags.HasTag(Stats.invulnerableTag);
        #endregion
        
        #region Pool
        private IObjectPool<EntityController> _objectPool;
        private bool _isReleased;
        public void AssignToPool(IObjectPool<EntityController> objectPool) => _objectPool = objectPool;
        private void ReturnToPool() { _isReleased = true; _objectPool.Release(this); }
        #endregion
    
        #region Pool lifecycle helpers
        
        // Called by pool on Get (actionOnGet)
        public void OnSpawn()
        {
            _isReleased = false;
            
            if(!Stats)
                throw new System.Exception("[EntityController] Stats not set!");

            InitializeAllSubsystems();
            
            // Rebind ONLY here
            Animator.Rebind();
            Animator.Update(0f);
        }

        // Called by pool on Release (actionOnRelease)
        public void OnDespawn()
        {
            DeinitializeAllSubsystems();
            
            Tags.ClearTags();
        }

        #endregion
    }
}
