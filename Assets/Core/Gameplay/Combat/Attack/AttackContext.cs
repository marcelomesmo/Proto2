using Core.Gameplay.Entity;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public struct AttackContext
    {
        public EntityController Target;
        public Vector2 Direction;
    }
}
