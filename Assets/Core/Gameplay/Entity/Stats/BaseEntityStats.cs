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
        
        [Header("Stats")]
        public int maxHealth = 100;
    
        [Header("Movement")]
        public float moveSpeed = 2f;
        public float speedMult = 1f;
    
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
