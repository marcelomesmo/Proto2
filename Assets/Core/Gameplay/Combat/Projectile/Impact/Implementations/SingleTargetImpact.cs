using Core.Gameplay.Combat.Attack;
using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Combat.Projectile.Impact.Implementations
{
    [CreateAssetMenu(fileName = "SingleTargetImpact", menuName = "Combat/Projectile/Impact/Single Target")]
    public class SingleTargetImpact : ProjectileImpact
    {
        public override void OnImpact(
            ProjectileInstance projectileInstance,
            Collider2D other,
            DamagePayload payload)
        {
            if (!other.TryGetComponent<IDamageable>(out var damageable))
                return;

            if (damageable.Faction == projectileInstance.GetOwnerFaction())
                return;

            if (!damageable.CanBeDamaged())
                return;

            damageable.TakeDamage(payload);
        }
    }
}
