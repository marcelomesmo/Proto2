using UnityEngine;

public class AutoDestroyVFX : MonoBehaviour
{
    void Start()
    {
        // TBD: Move the ps in the prefab to the root (it's in the child now), think of ways to redo this using animations instead of PS
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            // Fallback: destroy after 2 seconds
            Destroy(gameObject, 2f);
        }
    }
}