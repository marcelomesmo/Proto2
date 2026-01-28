using Core.Services.Manager;
using UnityEngine;

namespace Core.VFX
{
    public class PooledParticleSystemVFX : MonoBehaviour
    {
        [Header("Particle System")]
        [SerializeField] private ParticleSystem[] particleSystems;
        
        private GameObject _prefab;

        public void Initialize(GameObject prefab)
        {
            _prefab = prefab;

            if (particleSystems == null || particleSystems.Length == 0)
            {
                particleSystems = GetComponentsInChildren<ParticleSystem>();
            }
        }

        public void Play()
        {
            foreach (var ps in particleSystems)
            {
                ps.Clear(true);
                ps.Play(true);
            }
        }

        private void OnParticleSystemStopped()
        {
            // IMPORTANT: Stop Action must be "Callback"
            if (_prefab != null)
            {
                VFXPoolManager.Instance.Release(_prefab, gameObject);
            }
        }
    }
}
