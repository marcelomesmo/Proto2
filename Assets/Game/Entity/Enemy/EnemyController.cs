using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Tags;
using Entity;
using UnityEngine;
using UnityEngine.Pool;

namespace Enemy
{
    public class EnemyController : EntityController
    {
        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == Stats.deadTag)
                OnDeath();
        }

        private void OnDeath()
        {
            //Debug.Log("[BaseEnemyController] Enemy died.");
            
            // This is now only valid if Controller:
            // - Notifies a wave manager
            // - Signals an encounter controller
            // - Informs a boss phase system
            // - Coordinates multiple entities
            
            Tags.ClearTemporaryTags();
        }

        public override void NotifyDeathAnimationFinished()
        {
            // HARD INVARIANT: only valid if already dead
            if (!IsDead)
                return;
            
            if (_isReleased)
                return;

            ReturnToPool();
        }
        
        #region Pool
        private IObjectPool<EnemyController> _objectPool;
        private bool _isReleased;
        public void AssignToPool(IObjectPool<EnemyController> objectPool) => _objectPool = objectPool;
        private void ReturnToPool() { _isReleased = true; _objectPool.Release(this); }
        #endregion
    
        #region Pool lifecycle helpers
        
        // Called by pool on Get (actionOnGet)
        public void OnSpawn()
        {
            _isReleased = false;
            
            if(!Stats)
                throw new System.Exception("[BaseEnemyController] Stats not set!");

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