using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Upgrades
{
    public enum RequirementMode
    {
        AnyOf = 0,
        AllOf = 1
    }
    
    [Serializable]
    public sealed class UpgradeUnlockRule
    {
        [SerializeField]
        private RequirementMode mode = RequirementMode.AnyOf;

        [SerializeField]
        private List<UpgradeRequirement> requirements = new();

        public RequirementMode Mode => mode;

        public IReadOnlyList<UpgradeRequirement> Requirements =>
            requirements;
    }
}