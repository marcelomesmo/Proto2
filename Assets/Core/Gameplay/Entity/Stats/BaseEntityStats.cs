using Core.Enum;
using Core.Gameplay.Entity.Tags;
using UnityEngine;

namespace Core.Gameplay.Entity.Stats
{
    public abstract class BaseEntityStats : ScriptableObject
    {
        [Header("Definition")]
        public string characterId;
        public Faction faction;
    
        [Header("Combat Stats")]
        public float attackPower = 10f;
        public float healingPower = 10f;
        public float defense = 1f;
        
        [Header("Targeting")]
        public LayerMask combatEntityLayers;
        public float aggroRadius = 8f;
        public float aggroTolerance = 10f;
        
        [Header("Stats")]
        public int maxHealth = 100;
    
        [Header("Movement")]
        public float moveSpeed = 2f;
        public float speedMult = 1f;
        
        [Header("Actions")]
        public float thinkingTime = 1.5f;       // Could maybe rename this to scanInterval since "thinking is an enemy-flavoured word
        public float globalCooldown = 0.5f;
    
        [Header("Tags")]
        public GameplayTag invulnerableTag;
        public GameplayTag spawnFinishedTag;
        public GameplayTag matchEndedTag;
        public GameplayTag deadTag;
        
        public GameplayTag stunTag;
        public GameplayTag slowTag;
        public GameplayTag burnTag;
    }
}
