using Core.Enum;
using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Game.Entity.Player.Stats
{
    [CreateAssetMenu(menuName = "Game/Stats - Character")]
    public class CharacterStats : BaseEntityStats
    {
        [Header("Actions")]
        public float thinkingTime = 1.5f;
        public float globalCooldown = 0.5f;
        
        [Header("Targeting")]
        public LayerMask combatEntityLayers;
        
        public CharacterStats() { faction = Faction.Player; }
    }
}