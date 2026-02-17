using UnityEngine;

namespace Core.Upgrades.Effects
{
    public abstract class UpgradeEffect : ScriptableObject
    {
        public abstract void Apply(UpgradeContext context);
        public abstract void Remove(UpgradeContext context);
    }
}
