using UnityEngine;

namespace Core.VFX
{
    /*
     *      DEPRECATED
     *
     *      Only used when no pool implemented.
     */
    public class AutoDestroyVFX : MonoBehaviour
    {
        private void Start()
        {
            var ps = GetComponent<ParticleSystem>();
            if (ps)
                Destroy(gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
            else
                // Fallback: destroy after 2 seconds
                Destroy(gameObject, 2f);
        }
    }
}