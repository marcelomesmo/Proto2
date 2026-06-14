using System.Collections.Generic;
using Core.Enum;
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
        [Tooltip("In AoE defines multihit.")]
        [SerializeField] protected int targetSplashLimit = 5;
        
        private readonly List<Collider2D> _results = new();

        private int _entitiesHit = 0;

        public override void OnImpact(
            ProjectileInstance projectileInstance, 
            Collider2D other, 
            CombatPayload payload)
        {
            _results.Clear();

            _entitiesHit = 0;
            
            var filter = payload.source.targetFilter;
            
            Physics2D.OverlapCircle(
                projectileInstance.transform.position,
                radius,
                filter.ToContactFilter(),
                _results);
            
            // todo: order the hits by distance before iterating, otherwise we are getting enemies too further away.
            
            foreach (var hit in _results)
            {
                if (!filter.CanHit(hit))
                    continue;
                
                if (!hit.TryGetComponent<ICombatReceiver>(out var receiver))
                    continue;

                if (payload.action == CombatAction.Damage &&
                    receiver.Faction == projectileInstance.GetOwnerFaction())     // Damage ignores allies
                    continue;
                
                if (payload.action == CombatAction.Heal &&
                    receiver.Faction != projectileInstance.GetOwnerFaction())     // Healing ignores enemies
                    continue;

                if (!receiver.CanReceiveCombat(payload))
                    continue;

                CombatExecutionPipeline.Execute(
                    receiver,
                    payload);
                
                // Reached maximum splash hits
                _entitiesHit++;
                if (_entitiesHit >= targetSplashLimit)
                    break;
            }
        }
    }
}
