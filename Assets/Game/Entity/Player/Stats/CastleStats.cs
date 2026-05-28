using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Game.Entity.Player.Stats
{
    [CreateAssetMenu(fileName = "Castle Stats", menuName = "Game/Stats - Castle")]
    public class CastleStats : BaseEntityStats
    {
        [Header("Game Stats")] 
        public float lootBonus = 1f;
        public float expBonus = 1f;
    }
}
