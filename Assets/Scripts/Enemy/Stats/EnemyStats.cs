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
        public float thinkingTime = 0.3f;
        
        [Header("Base Movement")]
        public EnemyMovementType movementType = EnemyMovementType.Ground;
        public float preferredDistance = 1f;

        [Header("Dash Settings")]
        public float dashSpeedMultiplier = 2f;
        public float dashDelay = 0.5f;
        public float dashCooldown = 3f;

        [Header("Flying Settings")]
        public float flyHoverHeight = 1f;

        [Header("Combat")]
        public float attackRange = 1.5f;
        public float attackCooldown = 1f;

        [Header("Melee Combat")]
        public int contactDamage = 10;
        
        [Header("Ranged Combat")]
        public bool isRanged = false;
        public GameObject projectilePrefab;
        
        [Header("Targeting")]
        public float aggroRadius = 8f;
        public float aggroTolerance = 10f;
        public LayerMask entityMask;
        
        public EnemyStats() { faction = Faction.Enemy; }
    }
}