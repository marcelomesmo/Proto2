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
                    return source.sourceEntity.IsAlly(target);


                case CombatTargetType.Enemies:
                    return source.sourceEntity.IsEnemy(target);


                case CombatTargetType.Self:
                    return source.sourceEntity == target;


                case CombatTargetType.Any:
                    return true;


                default:
                    return false;
            }
        }
    }
}