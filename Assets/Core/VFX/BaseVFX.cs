using Core.Services.Manager;
using UnityEngine;

namespace Core.VFX
{
    public class BaseVFX : MonoBehaviour
    {
        [Header("Pool management")]
        [SerializeField] protected float lifetime = 3f;
    
        protected float Timer;
        protected GameObject PrefabReference;
        protected bool Initialized;
    
        private void Update()
        {
            if (!Initialized)
                return;

            Timer -= Time.deltaTime;
        
            OnUpdate();

            if(Timer <= 0f)
            {
                VFXPoolManager.Instance.Release(PrefabReference, gameObject);
                Initialized = false;
            }
        }

        protected virtual void OnUpdate() { }
    }
}
