using Gameplay.Combat;
using UnityEngine;

namespace Gameplay.Projectile
{
    public readonly struct DamagePayload
    {
        // What happened
        public readonly AttackData hitData;
        public readonly Vector2 hitPoint;
        public readonly DamageSource source;

        public DamagePayload(
            AttackData hitData,
            Vector2 hitPoint,
            DamageSource source)
        {
            this.hitData = hitData;
            this.hitPoint = hitPoint;
            this.source = source;
        }
    }
}