using Core.Gameplay.Combat.Attack;
using UnityEngine;

namespace Core.Gameplay.Combat.Projectile.Impact
{
    public abstract class ProjectileImpact : ScriptableObject
    {
        [Header("Base Settings")]
        [SerializeField] protected int targetLimit = 1;
    
        public virtual int GetMaxTargets(ProjectileContext context)
        {
            // context can include player stats, upgrades, etc.
            return targetLimit + context.bonusPierce;   // eg. on how to scale this later to match player upgrades.
        }
        
        public abstract void OnImpact(
            ProjectileInstance projectileInstance, 
            Collider2D other, 
            DamagePayload payload);
    }
}
