using System;
using UnityEngine;

namespace Core.Upgrades
{
    [Serializable]
    public sealed class UpgradeRequirement
    {
        [SerializeField]
        private UpgradeDefinition upgrade;

        [SerializeField]
        [Min(1)]
        private int requiredLevel = 1;

        public UpgradeDefinition Upgrade => upgrade;
        public int RequiredLevel => requiredLevel;
    }
}