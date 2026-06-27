using Core.Enum;
using Core.Gameplay.Combat.Attack;

namespace Core.Interfaces
{
    public interface ICombatReceiver
    {
        Faction Faction { get; }
        bool CanReceiveCombat(CombatPayload payload);
        void ReceiveDamage(CombatPayload context);
        void ReceiveHeal(CombatPayload context);
        void ReceiveStatusEffect(CombatPayload payload);
    }
}