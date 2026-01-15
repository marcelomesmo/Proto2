using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Game.Entity.Player.Stats
{
    [CreateAssetMenu(menuName = "Game/Character Stats")]
    public class CharacterStats : BaseEntityStats
    {
        [Header("Game Stats")] 
        public float attackPower = 10f;
        public float defense = 10f;
        
        //public CharacterStats() { faction = Faction.Player; }
    }
}