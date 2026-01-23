using UnityEngine;

namespace Core.Gameplay.Combat.AreaEffect
{
    public enum AreaShape
    {
        Circle,
        Box,
        Cone // optional later
    }
    
    [CreateAssetMenu(menuName = "Combat/Area Effect/Area Effect Data")]
    public class AreaEffectData : ScriptableObject
    {
        [Header("Shape")]
        public AreaShape shape;
        public float radius;        // Used by Circle
        public Vector2 boxSize;     // Used by Box

        [Header("Timing")]
        public float duration;
        public float tickInterval = 1f;

        [Header("Targeting")]
        public LayerMask hitLayers;
        
        [Header("Visuals")]
        public GameObject vfxPrefab;
        public bool followOwner = false;
    }
}
