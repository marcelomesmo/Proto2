using UnityEngine;

namespace Gameplay.Loot
{
    public interface IMagnetizableLoot
    {
        void ApplyMagnet(Vector2 sourcePosition, float strength);
    }
}