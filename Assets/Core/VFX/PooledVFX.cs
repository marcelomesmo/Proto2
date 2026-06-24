using UnityEngine;
using UnityEngine.Pool;

namespace Core.VFX
{
    public abstract class PooledVFX : MonoBehaviour
    {
        private IObjectPool<PooledVFX> _pool;
        private bool _isReleased;

        public bool IsReleased => _isReleased;

        public void AssignToPool(IObjectPool<PooledVFX> pool)
        {
            _pool = pool;
        }

        public virtual void OnSpawn()
        {
            _isReleased = false;
        }

        public virtual void OnDespawn() { }

        public void Release()
        {
            ReturnToPool();
        }
        
        protected void ReturnToPool()
        {
            if (_isReleased || _pool == null)
                return;

            _isReleased = true;
            _pool.Release(this);
        }
    }
}
