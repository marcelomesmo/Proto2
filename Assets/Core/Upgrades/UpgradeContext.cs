using System.Collections.Generic;
using Core.Services.Meta;
using Core.Upgrades.Effects;

namespace Core.Upgrades
{
    public sealed class UpgradeContext
    {
        public readonly UpgradeManager Manager;
        
        private readonly List<ExperienceModifier> _xpModifiers =
            new();

        public UpgradeContext(UpgradeManager manager)
        {
            Manager = manager;
        }
        
        // ---------------- XP ----------------

        public void RegisterExperienceModifier(
            ExperienceModifier modifier)
        {
            _xpModifiers.Add(modifier);
        }

        public void UnregisterExperienceModifier(
            ExperienceModifier modifier)
        {
            _xpModifiers.Remove(modifier);
        }

        public IReadOnlyList<ExperienceModifier> ExperienceModifiers =>
            _xpModifiers;
    }
}
