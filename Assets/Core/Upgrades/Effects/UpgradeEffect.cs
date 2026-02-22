using UnityEngine;

namespace Core.Upgrades.Effects
{
    public abstract class UpgradeEffect : ScriptableObject
    {
        // level is the purchased level for this upgrade during the match.
        // Convention: level <= 0 => do nothing.
        public abstract void Apply(UpgradeContext context, int level);
        public abstract void Remove(UpgradeContext context, int level);
    }
}
