using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.AreaEffect;
using Core.Gameplay.Combat.ChainAttack;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    [CreateAssetMenu(fileName = "Attack Data", menuName = "Combat/Attack Data")]
    public class AttackData : ScriptableObject
    {
        [Header("Execution")]
        public AttackExecutionMode executionMode = AttackExecutionMode.Melee;
        
        [Header("Base Settings")]
        public int damage;
        public float range;
        public float cooldown;
        public HitType hitType;
        
        public GameObject hitVFX;
        public GameObject castVFX;

        [Header("Ranged - Settings")]
        public GameObject projectilePrefab;
        public ProjectileDirectionMode directionMode;
        public float fixedAngle;
        
        [Header("Area Effect - Settings")]
        public AreaEffectData areaEffectData;
        
        [Header("Chain Attack - Additional Settings")]
        public ChainAttackData chainData;
        
        [Header("Side Effects (Buffs/Debuffs)")]
        [SerializeField] private List<AttackEffectData> effects = new();
        
        public IReadOnlyList<AttackEffectData> Effects => effects;
    }
}
