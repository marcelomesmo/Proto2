using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.AreaEffect;
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
        
        public GameObject hitVFX;   // hit vfx overload? todo: hit vfx by hittype?

        [Header("Ranged - Settings")]
        public GameObject projectilePrefab;
        public ProjectileDirectionMode directionMode;
        public float fixedAngle;
        
        [Header("Area Effect - Settings")]
        public AreaEffectData areaEffectData;
        
        [Header("Side Effects (Buffs/Debuffs)")]
        [SerializeField] private List<AttackEffectData> effects = new();
        
        public IReadOnlyList<AttackEffectData> Effects => effects;
    }
}
