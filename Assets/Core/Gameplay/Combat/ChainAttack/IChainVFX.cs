using UnityEngine;

namespace Core.Gameplay.Combat.ChainAttack
{
    public interface IChainVfx
    {
        void Initialize(Transform from, Transform to);
    }
}