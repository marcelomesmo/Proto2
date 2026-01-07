using Entity.Tags;
using UnityEngine;

namespace Entity
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
    }
}
