using UnityEngine;

namespace Entity.Tags
{
    [CreateAssetMenu(menuName = "Tags/Gameplay Tag")]
    public class GameplayTag : ScriptableObject
    {
        // Nothing here — tag identity comes from the asset instance
        public TagLifetime lifetime;
    }
    
    [System.Serializable]
    public enum TagLifetime
    {
        Persistent,   // dead, invulnerable
        Temporary     // stun, knockback, slow
    }
}