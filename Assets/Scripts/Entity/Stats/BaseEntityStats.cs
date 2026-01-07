using Entity.Tags;
using UnityEngine;

public abstract class BaseEntityStats : ScriptableObject
{
    [Header("Common Stats")]
    [Header("Definition")]
    public Faction faction;
    
    [Header("Health")]
    public int maxHealth = 100;
    
    [Header("Movement")]
    public float moveSpeed = 2f;
    
    [Header("Jump Settings")]
    public float jumpHeight = 3f;
    public float jumpCooldown = 2f;
    
    [Header("Tags")]
    public GameplayTag invulnerableTag;
    public GameplayTag spawnFinishedTag;
    public GameplayTag stunnedTag;
    public GameplayTag deadTag;
}
