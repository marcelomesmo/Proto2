using UnityEngine;

namespace Core.Gameplay.Combat.Projectile.Movement
{
    public interface IProjectileTargetReceiver
    {
        void SetTarget(Transform target);
    }
}