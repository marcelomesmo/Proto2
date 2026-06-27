using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.ChainAttack;
using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Combat
{
    public static class CombatExecutionPipeline
    {
        public static void Execute(
            ICombatReceiver receiver,
            CombatPayload payload)
        {
            // TODO: This becomes the gate to weather we can execute or not, we can remove
            //      receiver.CanReceiveCombat() checks from impact/area/projectile paths later.
            if (receiver == null) return;
            if (!receiver.CanReceiveCombat(payload)) return;
            
            ExecutePayload(receiver, payload);

            ResolveChain(receiver, payload);
        }
        
        public static void ExecutePayload(
            ICombatReceiver receiver,
            CombatPayload payload)
        {
            switch(payload.action)
            {
                case CombatAction.Damage:
                    receiver.ReceiveDamage(payload);
                    break;

                case CombatAction.Heal:
                    receiver.ReceiveHeal(payload);
                    break;
                
                case CombatAction.ApplyStatus:
                    receiver.ReceiveStatusEffect(payload);
                    break;
                
                default:
                    Debug.LogWarning(
                        $"[CombatResolver] Unhandled combat action {payload.action}");
                    break;
            }
        }

        private static void ResolveChain(
            ICombatReceiver receiver,
            CombatPayload payload)
        {
            if(payload.attack == null)
                return;

            if(payload.chainDepth > 0)
                return;

            if(!payload.attack.Data.chainData)
                return;

            ChainAttackResolver.ResolveChain(
                payload.attack,
                payload,
                receiver);
        }
    }
}