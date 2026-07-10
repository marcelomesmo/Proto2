using UnityEngine;

namespace Core.VFX
{
    public class PooledTimedVFX : PooledVFX
    {
        [Header("Pool management")]
        [SerializeField] protected float lifetime = 3f;
    
        protected float Timer;
        protected bool Active;
    
        private void Update()
        {
            if (!Active)
                return;

            Timer -= Time.deltaTime;
            OnUpdate();

            if(Timer <= 0f)
            {
                Active = false;
                ReturnToPool();
            }
        }
        
        public override void OnSpawn()
        {
            base.OnSpawn();
            
            Timer = lifetime;
            Active = true;
        }

        public override void OnDespawn()
        {
            base.OnDespawn();
            
            Active = false;
            Timer = 0f;
        }

        protected virtual void OnUpdate() { }
    }
}
