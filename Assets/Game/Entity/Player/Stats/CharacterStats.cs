using Core.Enum;
using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Game.Entity.Player.Stats
{
    [CreateAssetMenu(menuName = "Game/Stats - Character")]
    public class CharacterStats : BaseEntityStats
    {
        [Header("Game Stats")] 
        public float attackPower = 10f;
        public float defense = 10f;
        public float speedMult = 1f;
        
        [Header("Actions")]
        public float thinkingTime = 1.5f;
        public float globalCooldown = 0.5f;
        
        [Header("Targeting")]
        public LayerMask combatEntityLayers;
        
        public CharacterStats() { faction = Faction.Player; }
    }
}