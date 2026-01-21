using UnityEngine;

namespace Core.Interfaces
{
    public interface IMagnetizableLoot
    {
        void TryBeginMagnet(Transform target, float speedMultiplier);
    }
}