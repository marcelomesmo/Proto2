using UnityEngine;

namespace Core.Services.Meta
{
    public abstract class MatchStatsProvider : ScriptableObject
    {
        public abstract MatchStats CreateStats();
    }
}
