using System;

// Runtime representation of an upgrade during a match.
// Carries definition + purchased level (already clamped/validated by loader).
namespace Core.Upgrades.Runtime
{
    [Serializable]
    public readonly struct RuntimeUpgrade
    {
        public readonly UpgradeDefinition Definition;
        public readonly int Level;

        public RuntimeUpgrade(UpgradeDefinition definition, int level)
        {
            Definition = definition;
            Level = level;
        }
    }
}
