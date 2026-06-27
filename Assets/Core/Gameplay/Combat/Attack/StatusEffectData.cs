using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Modifiers;
using Core.Gameplay.Entity.Tags;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    [CreateAssetMenu(menuName = "Combat/Status Effect")]
    public class StatusEffectData : ScriptableObject
    {
        [Header("Effect")]
        public StatusEffectType effectType;

        [Header("Lifetime")]
        public float duration = 1f;
        public bool isPermanent = false;

        [Header("Magnitude")]
        public float value;                 // slow %, damage per tick, etc.

        [Header("Ticking (optional)")]
        public float tickInterval = 1f;     // 1 = 1 sec ticks, 0 = instant / non-ticking
        
        [Header("Stat Modifiers (StatusEffectType.StatModifiers")]
        public List<AttackStatModifier> statModifiers = new();

        [Header("Tag (optional — applied for duration, e.g. for animation)")]
        public GameplayTag tagToApply;
    }
}
