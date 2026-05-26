using Core.Enum;
using Core.Gameplay.Entity.Tags;
using UnityEngine;

namespace Core.Gameplay.Entity.Stats
{
    public abstract class BaseEntityStats : ScriptableObject
    {
        [Header("Common Stats")]
        [Header("Definition")]
        public string characterId;
        public Faction faction;
    
        [Header("Stats")]
        public int maxHealth = 100;
    
        [Header("Movement")]
        public float moveSpeed = 2f;
    
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
