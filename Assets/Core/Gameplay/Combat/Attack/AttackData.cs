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
        [Header("Presentation")]
        public string displayName;
        public Sprite icon;
        public string description;
        
        [Header("Execution")]
        public AttackExecutionMode executionMode = AttackExecutionMode.Melee;
        public CombatAction combatActionType = CombatAction.Damage;
        
        [Header("Base Settings")]
        public int amount;
        public float range;
        public float cooldown;
        public HitTypes hitTypes;
        
        public GameObject hitVFX;
        public GameObject castVFX;
        
        [Header("Targeting")]
        public CombatTargetType targetType;
        public TargetSelectionMode targetSelectionMode = TargetSelectionMode.Closest;
        public TargetRequirement targetRequirement = TargetRequirement.None;
        public LayerMask targetLayers;

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
