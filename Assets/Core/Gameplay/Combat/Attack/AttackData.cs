using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.AreaAttack;
using Core.Gameplay.Combat.ChainAttack;
using Core.VFX;
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
        
        public PooledVFX hitVFX;
        public PooledVFX castVFX;
        
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
        public AreaAttackData areaAttackData;
        
        [Header("Chain Attack - Additional Settings")]
        public ChainAttackData chainData;
        
        [Header("Side Effects (Buffs/Debuffs)")]
        [SerializeField] private List<StatusEffectData> effects = new();
        
        public IReadOnlyList<StatusEffectData> Effects => effects;
        
        [Header("Passive / Silent")]
        [Tooltip("Cast once on init/receive, never in normal loop.")]
        public bool isPassive;
        [Tooltip("Suppresses animator trigger.")]
        public bool castSilently;
        
        [Header("Variant Info")]
        [Tooltip("If this AttackData is a variant (e.g. replaces another attack via AttackVariantModifier), " +
                 "set its original attack here so stat modifiers targeting the base attack still apply.")]
        public AttackData baseAttack;
    }
}
