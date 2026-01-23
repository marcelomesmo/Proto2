using System.Collections.Generic;
using Core.Gameplay.Combat.Attack;
using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Combat.Projectile.Impact.Implementations
{
    [CreateAssetMenu(fileName = "AreaDamageImpact", menuName = "Combat/Projectile/Impact/Area Damage: Explosion")]
    public class AreaDamageImpact : ProjectileImpact
    {
        [Header("Area Settings")]
        [SerializeField] private float radius = 5f;
        [SerializeField] private LayerMask affectedLayers;
        
        private readonly List<Collider2D> _results = new();

        public override void OnImpact(
            ProjectileInstance projectileInstance, 
            Collider2D other, 
            DamagePayload payload)
        {
            _results.Clear();
            
            var filter = new ContactFilter2D
            {
                useTriggers = true,
                layerMask = affectedLayers
            };
            
            Physics2D.OverlapCircle(
                projectileInstance.transform.position,
                radius,
                filter,
                _results);
            
            foreach (var hit in _results)
            {
                if (!hit.TryGetComponent<IDamageable>(out var damageable))
                    continue;

                if (damageable.Faction == projectileInstance.GetOwnerFaction())
                    continue;

                if (!damageable.CanBeDamaged())
                    continue;

                damageable.TakeDamage(payload);
            }
        }
    }
}
