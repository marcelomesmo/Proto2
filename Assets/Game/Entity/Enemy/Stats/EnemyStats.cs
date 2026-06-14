using Core.Enum;
using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Game.Entity.Enemy.Stats
{
    public enum EnemyMovementType
    {
        Ground,
        Lane,
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
        //public float preferredDistance = 1f;
        
        [Header("Targeting")]
        public float aggroRadius = 8f;
        public float aggroTolerance = 10f;
        public LayerMask combatEntityLayers;
        
        [Header("Attributes")]
        public int xpReward = 5;
        
        public EnemyStats() { faction = Faction.Enemy; }
    }
}