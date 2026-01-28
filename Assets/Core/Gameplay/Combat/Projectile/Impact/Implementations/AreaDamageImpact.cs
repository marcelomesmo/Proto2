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
        [SerializeField] protected int targetSplashLimit = 5;
        
        private readonly List<Collider2D> _results = new();

        private int _entitiesHit = 0;

        public override void OnImpact(
            ProjectileInstance projectileInstance, 
            Collider2D other, 
            DamagePayload payload)
        {
            _results.Clear();

            _entitiesHit = 0;
            
            var filter = payload.source.targetFilter;
            
            Physics2D.OverlapCircle(
                projectileInstance.transform.position,
                radius,
                filter.ToContactFilter(),
                _results);
            
            foreach (var hit in _results)
            {
                if (!filter.CanHit(hit))
                    continue;
                
                if (!hit.TryGetComponent<IDamageable>(out var damageable))
                    continue;

                if (damageable.Faction == projectileInstance.GetOwnerFaction())
                    continue;

                if (!damageable.CanBeDamaged())
                    continue;

                damageable.TakeDamage(payload);
                
                // Reached maximum splash hits
                _entitiesHit++;
                if (_entitiesHit >= targetSplashLimit)
                    break;
            }
        }
    }
}
