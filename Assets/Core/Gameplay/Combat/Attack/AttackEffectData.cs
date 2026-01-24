using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public enum StatusEffectType
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
        public StatusEffectType effectType;

        [Header("Timing")]
        public float duration = 1f;

        [Header("Magnitude")]
        public float value; // slow %, damage per tick, etc.

        [Header("Ticking (optional)")]
        public float tickInterval = 1f; // 1 = 1 sec ticks, 0 = instant / non-ticking
    }
}
