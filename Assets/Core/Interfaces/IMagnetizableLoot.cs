using UnityEngine;

namespace Core.Interfaces
{
    public interface IMagnetizableLoot
    {
        void ApplyMagnet(Vector2 sourcePosition, float strength);
    }
}