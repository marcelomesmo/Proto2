using Core.Enum;
using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Game.Entity.Player.Stats
{
    [CreateAssetMenu(menuName = "Game/Stats - Character")]
    public class CharacterStats : BaseEntityStats
    {
        public CharacterStats() { faction = Faction.Player; }
    }
}