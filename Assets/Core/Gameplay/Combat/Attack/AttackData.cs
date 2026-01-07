using Core.Enum;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    [CreateAssetMenu(fileName = "Attack Data", menuName = "Combat/Attack Data")]
    public class AttackData : ScriptableObject
    {
        public int damage;
        public HitType hitType;
        public GameObject hitVFX;
        
        // FUTURE IMPROVEMENTS
        /*
            Create a damage info payload:

            public struct DamageInfo
            {
                public int amount;
                public Vector2 hitPoint;
                public Vector2 direction;
                public DamageType type;
                public bool isCritical;
                public GameObject source;
            }
        */
    }
}
