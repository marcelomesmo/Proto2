using Gameplay.Projectile;
using UnityEngine;

namespace Interfaces
{
    public interface IDamageable
    {
        Faction Faction { get; }
        bool CanBeDamaged();
        void TakeDamage(DamagePayload context);
    }
}