using Core.Services.Meta;
using UnityEngine;

namespace Game.Services.Meta
{
    [CreateAssetMenu(fileName = "GameMatchStatsProvider", menuName = "Game/Factory - Match Stats Provider")]
    public sealed class GameMatchStatsProvider : MatchStatsProvider
    {
        public override MatchStats CreateStats()
        {
            return new GameMatchStats();
        }
    }
}
