using System.Collections.Generic;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity.Subsystem;

namespace Core.Gameplay.Combat
{
    public static class AttackVariantResolver
    {
        public static AttackInstance Resolve(
            AttackInstance sourceAttack,
            AttackContext context,
            IReadOnlyList<AttackVariantModifier> modifiers,
            EntityAttackSubsystem attackSubsystem)
        {
            AttackVariantModifier? selectedVariant = null;
            
            foreach (var variant in modifiers)
            {
                // Filter application
                if (!variant.AppliesTo(sourceAttack, context))
                    continue;

                if (variant.replacementAttack == null)  // Invalid config filter
                    continue;
                
                // Select the highest priority variant.
                if (selectedVariant == null)
                {
                    selectedVariant = variant;
                    continue;
                }

                if (variant.priority >
                    selectedVariant.Value.priority)
                {
                    selectedVariant = variant;
                }
            }
            
            // Return a new instance created from the VariantModifier.
            if (selectedVariant.HasValue)
            {
                return attackSubsystem.GetOrCreateVariantInstance(
                    selectedVariant.Value.replacementAttack);
            }

            return sourceAttack;    // Otherwise, just proceed with the base AttackInstance.
        }
    }
}
