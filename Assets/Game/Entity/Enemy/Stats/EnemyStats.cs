using Core.Enum;
using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Enemy
{
    public enum EnemyMovementType
    {
        Ground,
        Flyer,
        Jumper,
        Dasher
    }
    
    [CreateAssetMenu(menuName = "Stats/Enemy Stats")]
    public class EnemyStats : BaseEntityStats
    {
        [Header("Enemy Only")] 
        [Header("AI")] 
        public float thinkingTime = 1.5f;
        public float globalCooldown = 0.5f;
        
        [Header("Base Movement")]
        public EnemyMovementType movementType = EnemyMovementType.Ground;
        public float preferredDistance = 1f;

        [Header("Dash Settings")]
        public float dashSpeedMultiplier = 2f;
        public float dashDelay = 0.5f;
        public float dashCooldown = 3f;

        [Header("Flying Settings")]
        public float flyHoverHeight = 1f;
        
        [Header("Targeting")]
        public float aggroRadius = 8f;
        public float aggroTolerance = 10f;
        public LayerMask entityMask;
        
        public EnemyStats() { faction = Faction.Enemy; }
    }
}