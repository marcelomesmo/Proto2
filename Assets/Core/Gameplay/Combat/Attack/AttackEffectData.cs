using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public enum AttackEffectType
    {
        Stun,
        Slow,
        Burn
        // extensible
    }
    
    [CreateAssetMenu(menuName = "Combat/Attack Effect")]
    public class AttackEffectData : ScriptableObject
    {
        [Header("Effect")]
        public AttackEffectType effectType;

        [Header("Timing")]
        public float duration;

        [Header("Magnitude")]
        public float value; // slow %, damage per tick, etc.

        [Header("Ticking (optional)")]
        public float tickInterval; // 0 = instant / non-ticking
    }
}
