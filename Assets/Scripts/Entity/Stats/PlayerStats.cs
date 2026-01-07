using UnityEngine;

namespace Entity.Stats
{
    
    [CreateAssetMenu(menuName = "Stats/Player Stats")]
    public class PlayerStats : BaseEntityStats
    {
        [Header("Player Only")]
        [Header("Advanced Movement")]
        public float shootingSpeedMultiplier = 0.4f;
        public float sprintSpeedMultiplier = 1.4f;
        public float sprintStaminaTime = 1f;
        public float sprintRegenRate = 0.05f;
        
        public PlayerStats() { faction = Faction.Player; }
    }
}
