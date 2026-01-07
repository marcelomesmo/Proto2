using UnityEngine;

namespace Core.VFX
{
    public class ParticleSystemVFX : BaseVFX
    {
        [Header("Particle System")]
        [SerializeField] private ParticleSystem particleSystem;
        
        public void Initialize(GameObject prefab)
        {
            if(!particleSystem)
                particleSystem = GetComponentInChildren<ParticleSystem>();
            
            PrefabReference = prefab;
            Timer = particleSystem.main.duration;

            Initialized = true;
        }
    }
}
