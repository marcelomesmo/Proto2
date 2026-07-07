using UnityEngine;

namespace Core.VFX
{
    public class PooledParticleSystemVFX : PooledVFX
    {
        private ParticleSystem _particleSystem;
        private ParticleSystem[] _childPS;

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            if (!_particleSystem)
            {
                Debug.LogWarning(
                    $"[PooledParticleSystemVFX] Missing ParticleSystem on '{name}'", this);
                enabled = false;
                return;
            }

            _childPS = GetComponentsInChildren<ParticleSystem>();
        }
        
        // Called by ParticleSystem Stop Action = Callback
        public void OnParticleSystemStopped()
        {
            // IMPORTANT: Stop Action must be "Callback"
            
            // Stop all child particle systems
            if (_childPS is { Length: > 0 })
            {
                foreach (var ps in _childPS)
                {
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
            
            // Release this vfx
            ReturnToPool();
        }

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
        
        // Stops new emission but lets already-spawned particles finish naturally.
        // Requires Stop Action = Callback so OnParticleSystemStopped fires once actually finished.
        public void StopEmitting()
        {
            if (_particleSystem != null)
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        
        public override void OnDespawn()
        {
            if (_particleSystem != null)
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
