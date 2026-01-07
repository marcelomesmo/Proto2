using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Combat.Projectile.Impact
{
    // TODO: Make this abstract, check ProjectileMovement for reference.
    [CreateAssetMenu(fileName = "ProjectileImpact", menuName = "Projectile/Impact")]
    public class ProjectileImpact : ScriptableObject
    {
        [Header("Settings")]
        [SerializeField] protected int damage = 10;
        [SerializeField] protected int targetLimit = 1;
        
        // TODO: Move this to derived class.
        public bool isAoe = false;
        public float explosionRadius = 5f;  // is this world units?
    
        private List<Collider2D> _overlappingDestructibles = new();
    
        public virtual int GetMaxTargets(ProjectileContext context)
        {
            // context can include player stats, upgrades, etc.
            return targetLimit + context.bonusPierce;   // eg. on how to scale this later to match player upgrades.
        }
        
        public virtual int GetDamage(ProjectileContext context)
        {
            return (int)(damage * context.damageMultiplier);
        }
        
        //public abstract void OnImpact(GameObject bullet, Collider2D other, DamagePayload payload
        //);
        
        // TODO: Break this in two different ProjectileImpact, one for single target and one for aoe.
        public virtual void OnImpact(GameObject bullet, Collider2D other, DamagePayload payload)//Collision2D collision)
        {
            // Try to get projectile and its faction first
            var projectile = bullet.GetComponent<BaseProjectile>();
            var ownerFaction = projectile ? projectile.GetOwnerFaction() : Faction.Neutral;

            if (isAoe)
            {
                // This could be a public field and configured in the Inspector.
                // We can use this to filter by layer, contact normals, triggers etc.
                // Here we're doing no filtering at all so everything is returned.
                //var contactFilter = new ContactFilter2D().NoFilter();
                var contactFilter = new ContactFilter2D();
                contactFilter.SetLayerMask(LayerMask.GetMask("Player", "Enemy", "Destructible"));
                contactFilter.useTriggers = true;

                // Can later remove targetsHit as of right now it's only used for debugging.
                //var targetsHit = 0;
                
                _overlappingDestructibles.Clear();
                Physics2D.OverlapCircle(bullet.transform.position, explosionRadius, contactFilter, _overlappingDestructibles);
                // If AoE is frequent, prefer Physics2D.OverlapCircleNonAlloc into a fixed-size array to avoid allocations.
                
                // Apply damage to all objects in radius
                foreach (var hit in _overlappingDestructibles)
                {
                    if (!hit.TryGetComponent<IDamageable>(out var damageable))
                        continue;
                    
                    // Skip same-faction and neutral/ignored targets
                    if (damageable.Faction == ownerFaction)
                        continue;
                    
                    // Ask the damageable if it can currently be damaged (fast)
                    if (!damageable.CanBeDamaged())
                        continue;

                    damageable.TakeDamage(payload);
                    
                    // Apply damage (impact point heuristic: use actual hit point or center)
                    //damageable.TakeDamage(damage, hit.transform.position);
                    
                    //    damageable is BaseEnemyController 
                    //        ? hit.transform.position 
                    //        : bullet.transform.position);
                }
                
                //if(targetsHit > 0)
                //    Debug.Log("[ProjectileImpact] Dealing " + damage + " damage [AoE] to " + targetsHit + " targets.");
            }
            else
            {
                // Single target hit
                if (other != null && other.TryGetComponent<IDamageable>(out var damageable))
                {
                    if (damageable.Faction != ownerFaction && damageable.CanBeDamaged())
                    {
                        damageable.TakeDamage(payload);
                    }
                }
            }

            // Destroy the bullet. Pool handles this.
        }
    }
}
