using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;

namespace Core.Interfaces
{
    public interface IDamageable
    {
        Faction Faction { get; }
        bool CanBeDamaged();
        void TakeDamage(DamagePayload context);
        void ApplyAttackEffects(IReadOnlyList<AttackEffectData> effects, DamageSource source);
    }
}