using System.Collections.Generic;
using Core.Enum;

namespace Core.Gameplay.Combat.Attack
{
    public readonly struct CombatPayload
    {
        // What happened
        public readonly CombatAction action;
        public readonly AttackInstance attack;                             // null for DOT / effect damage
        public readonly int amount;                                 // ALWAYS set
        public readonly IReadOnlyList<AttackEffectData> effects;        // null for DOT / effect damage
        public readonly AttackSource source;
        public readonly int chainDepth;
        public readonly CombatFlags flags;
        
        public CombatPayload(
            CombatAction action,
            AttackInstance attack,
            int amount,
            IReadOnlyList<AttackEffectData> effects,
            AttackSource source,
            CombatFlags flags = CombatFlags.None,
            int chainDepth = 0)
        {
            this.action = action;
            this.attack = attack;
            this.amount = amount;
            this.effects = effects;
            this.source = source;
            this.flags = flags;
            this.chainDepth = chainDepth;
        }
        
        public bool IsCritical => (flags & CombatFlags.Critical) != 0;
        public bool HasResisted => (flags & CombatFlags.Resisted) != 0;
    }
}