using System;

namespace Core.Gameplay.Combat.Modifiers
{
    [Serializable]
    public struct HealthModifier
    {
        public ModifierType type;
        public float value;
    }
}
