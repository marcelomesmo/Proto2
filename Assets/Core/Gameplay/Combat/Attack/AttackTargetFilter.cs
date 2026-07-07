using Core.Enum;
using Core.Gameplay.Entity;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public sealed class AttackTargetFilter
    {
        public LayerMask layerMask;
        public bool allowTriggers;
        public CombatTargetType targetType;

        public ContactFilter2D ToContactFilter()
        {
            return new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = layerMask,
                useTriggers = allowTriggers
            };
        }
        
        public bool CanHit(Collider2D col)
        {
            if (!col)
                return false;

            // Extra safety — physics filter should already remove this
            if (((1 << col.gameObject.layer) & layerMask) == 0)
                return false;

            if (!allowTriggers && col.isTrigger)
                return false;
            
            return true;
        }
        
        public bool CanTarget(
            AttackSource source,
            EntityController target)
        {
            if (source?.sourceEntity == null || target == null)
                return false;

            switch (targetType)
            {
                case CombatTargetType.Allies:
                    return source.faction == target.Stats.faction;  // Same as comment below.

                case CombatTargetType.Enemies:
                    return source.faction != target.Stats.faction;
                        // source.sourceEntity.IsEnemy(target); // We can't delegate this to sourceEntity as we were doing before, as the owner can be dead when the projectile hits target. i.e. we can't rely on a living EntityController as source, it needs to be a snapshot.

                case CombatTargetType.Self:
                    return source.sourceEntity != null &&   // Safeguard in case the entity dies the frame it casts?
                           source.sourceEntity == target;

                case CombatTargetType.Any:
                    return true;

                default:
                    return false;
            }
        }
    }
}