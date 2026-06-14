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
            CombatPayload payload)
        {
            // Sanity check, but this is already handled in ProjectileInstance.
            if (!other.TryGetComponent<ICombatReceiver>(out var damageable))
                return;

            if (!damageable.CanReceiveCombat(payload))
               return;

            CombatExecutionPipeline.Execute(
                damageable,
                payload
            );
        }
    }
}
