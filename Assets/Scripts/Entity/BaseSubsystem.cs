using Audio;
using Entity.Tags;
using Services;
using UnityEngine;

namespace Entity
{
    public abstract class BaseSubsystem : MonoBehaviour
    {
        protected EntityController Controller { get; private set; }
        private bool _initialized;
        private bool _isActive;

        public bool IsActive => _initialized && _isActive;

        protected EntityAudioEmitter EntityAudio { get; private set; }
        
        public void Initialize(EntityController controller)
        {
            if (_initialized) return;
            _initialized = true;
            _isActive = true;
            
            Controller = controller;
            
            EntityAudio = GetComponent<EntityAudioEmitter>();

            // Directly subscribe using controller.Tags
            Controller.Tags.OnTagAdded += HandleTagAdded;
            Controller.Tags.OnTagRemoved += HandleTagRemoved;

            OnInitialize();
        }

        public void Deinitialize()
        {
            if (!_initialized) return;
            
            _initialized = false;
            _isActive = false;

            EntityAudio = null;

            Controller.Tags.OnTagAdded -= HandleTagAdded;
            Controller.Tags.OnTagRemoved -= HandleTagRemoved;
            
            OnDeinitialize();

            Controller = null;
        }
        
        private void Update()
        {
            if (!IsActive)
                return;
            
            if (PauseService.IsPaused)
                return;

            OnUpdate();
        }

        private void FixedUpdate()
        {
            if (!IsActive)
                return;

            OnFixedUpdate();
        }

        protected virtual void HandleTagAdded(GameplayTag tag) { }
        protected virtual void HandleTagRemoved(GameplayTag tag) { }

        protected virtual void OnInitialize() { }
        protected virtual void OnDeinitialize() { }

        protected virtual void OnUpdate() { }
        protected virtual void OnFixedUpdate() { }
    }
}