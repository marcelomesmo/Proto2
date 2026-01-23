using System.Collections.Generic;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    public readonly struct DamagePayload
    {
        // What happened
        public readonly AttackData hitData;
        public readonly IReadOnlyList<AttackEffectData> effects;
        public readonly Vector2 hitPoint;
        public readonly DamageSource source;

        public DamagePayload(
            AttackData hitData,
            IReadOnlyList<AttackEffectData> effects,
            Vector2 hitPoint,
            DamageSource source)
        {
            this.hitData = hitData;
            this.effects = effects;
            this.hitPoint = hitPoint;
            this.source = source;
        }
    }
}