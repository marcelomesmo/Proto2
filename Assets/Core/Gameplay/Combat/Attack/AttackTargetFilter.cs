using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public sealed class AttackTargetFilter
    {
        public LayerMask layerMask;
        public bool allowTriggers;

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
    }
}