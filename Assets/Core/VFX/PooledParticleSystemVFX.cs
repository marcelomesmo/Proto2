using Core.Services.Manager;
using UnityEngine;

namespace Core.VFX
{
    public class PooledParticleSystemVFX : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private ParticleSystem[] _childPS;
        
        private GameObject _prefab;

        public void Initialize(GameObject prefab)
        {
            _prefab = prefab;
            
            _particleSystem = GetComponent<ParticleSystem>();
            if (!_particleSystem)
            {
                Debug.LogWarning(
                    $"[PooledParticleSystemVFX] Missing Particle System in '{prefab.name}'",
                    this);
                enabled = false;
                return;
            }
            
            _childPS = GetComponentsInChildren<ParticleSystem>();
        }

        public void OnParticleSystemStopped()
        {
            // IMPORTANT: Stop Action must be "Callback"
            if (_prefab == null) 
                return;

            // Stop all child particle systems
            if (_childPS is { Length: > 0 })
            {
                foreach (var ps in _childPS)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
            
            // Release this vfx
            VFXPoolManager.Instance.Release(_prefab, gameObject);
        }
        
        #region Util

        // This is external only and not used right now.
        public void Play()
        {
            _particleSystem.Clear(true);
            _particleSystem.Play(true);
            
            /*foreach (var ps in _childPS)
            {
                ps.Clear(true);
                ps.Play(true);
            }*/
        }
        
        #endregion
    }
}
