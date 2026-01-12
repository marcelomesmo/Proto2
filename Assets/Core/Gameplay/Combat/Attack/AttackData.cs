using Core.Enum;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    [CreateAssetMenu(fileName = "Attack Data", menuName = "Combat/Attack Data")]
    public class AttackData : ScriptableObject
    {
        public int damage;
        public float range;
        public float cooldown;
        public HitType hitType;
        
        public GameObject hitVFX;

        public bool isRanged;
        public GameObject projectilePrefab;
        public ProjectileDirectionMode directionMode;
        public float fixedAngle;
    }
}
