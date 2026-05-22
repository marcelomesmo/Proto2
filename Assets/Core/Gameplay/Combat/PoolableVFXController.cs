using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Combat
{
    // Added to projectiles and area effects to Start/Clear persistent VFXs
    public sealed class PoolableVFXController : MonoBehaviour, IPoolableVisual
    {
        [SerializeField] private ParticleSystem[] playOnSpawn;
        [SerializeField] private ParticleSystem[] stopAndClearOnDespawn;
        [SerializeField] private ParticleSystem[] stopOnlyOnDespawn;

        public void OnSpawn()
        {
            foreach (var ps in playOnSpawn)
            {
                ps.Clear(true);
                ps.Play(true);
            }
        }

        public void OnDespawn()
        {
            foreach (var ps in stopAndClearOnDespawn)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            foreach (var ps in stopOnlyOnDespawn)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}