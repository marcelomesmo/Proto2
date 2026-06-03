using System;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public struct AttackVariantModifier
{
    public AttackData sourceAttack;
    public AttackData replacementAttack;

    // General filters
    public AttackVariantCondition condition;

    [Tooltip("Condition should be EveryNthCast(int), RandomChance(0 to 1)"), Min(0)]
    public float conditionValue;
    
    [Tooltip("If multiple variants match, the variant with the highest priority is selected. " +
             "If priorities are equal, the first registered modifier wins.")]
    public int priority;
    
    public bool AppliesTo(
        AttackInstance attack,
        AttackContext context)
    {
        // Attack filter
        if (sourceAttack != null &&
            sourceAttack != attack.Data)
            return false;

        // Condition filter
        switch (condition)
        {
            case AttackVariantCondition.EveryNthCast:
            {
                if(conditionValue <= 0) // input safety-check
                    return false;       // Debug.Log("[AttackVariantModifier] EveryNthCast condition shouldn't be zero."); 
                
                int nextCastNumber = attack.CastCount + 1;

                return nextCastNumber > 0 &&
                       nextCastNumber % (int)conditionValue == 0;
            }

            case AttackVariantCondition.RandomChance:
                return Random.value <= conditionValue;
            
            // Context is being passed here so later we can do stuff such as:
            // If target is Frozen -> Fireball becomes Steam Explosion
            // If target health < 10% -> Fireball becomes Execute Blast
            // However, I do feel these tag/state-like solutions should be in its own system (and not an upgrade variant).

            default:
                return false;
        }
    }
}
