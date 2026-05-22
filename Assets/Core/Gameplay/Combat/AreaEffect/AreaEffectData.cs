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
        public float circleRadius;        // Used by Circle
        public Vector2 boxSize;     // Used by Box
        public float coneAngle = 45f;   // degrees
        public float coneRadius = 4f;
        
        [Header("Timing")]
        public float duration;
        public float tickInterval = 1f;
        public bool isChanneled;

        [Header("Targeting")]
        public LayerMask hitLayers;
        
        [Header("Spawn")]
        public AreaSpawnMode spawnMode;
        public float forwardOffset; // distance in front of caster
        
        [Header("Visuals")]
        public GameObject areaEffectPrefab;
        public bool followOwner = false;
        public float fadeOutDuration = 0f;
    }
}

/*public enum OwnerDeathBehavior
{
    ImmediateDespawn,
    FinishDuration,
    DetachAndFade,
    Persist
}*/
