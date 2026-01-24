using UnityEngine;

namespace Core.Gameplay.Combat.ChainAttack
{
    [CreateAssetMenu(fileName = "ChainAttackData", menuName = "Combat/Chain Attack/Chain Attack Data")]
    public class ChainAttackData : ScriptableObject
    {
        public int maxBounces = 3;
        public float chainRange = 5f;

        [Tooltip("Multiplier applied per bounce (e.g. 0.8 = 20% loss per hop)")]
        public float damageMultiplierPerBounce = 0.8f;

        [Tooltip("Should effects be applied on every bounce")]
        public bool applyEffectsOnEveryBounce = true;
        
        [Header("Visuals")]
        public GameObject chainVfxPrefab;

        [Tooltip("Lifetime override for the VFX (0 = let prefab control it)")]
        public float vfxLifetime = 0.2f;
        
        [Header("Timing")]
        public float hopVfxDelay = 0f;
    }
}
