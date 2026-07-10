using UnityEngine;

namespace Core.Interfaces
{
    public interface IMagnetizableLoot
    {
        bool TryBeginMagnet(Transform target, float speedMultiplier);
    }
}