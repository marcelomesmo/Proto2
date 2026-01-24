using UnityEngine;

namespace Core.Gameplay.Entity.Tags
{
    [CreateAssetMenu(menuName = "Tags/Gameplay Tag")]
    public class GameplayTag : ScriptableObject
    {
        // Nothing here — tag identity comes from the asset instance
        public TagLifetime lifetime;

        public GameObject vfxPrefab;
    }
    
    [System.Serializable]
    public enum TagLifetime
    {
        Persistent,   // dead, invulnerable
        Temporary     // stun, knockback, slow
    }
}