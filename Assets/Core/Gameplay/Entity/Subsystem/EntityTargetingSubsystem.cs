using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using UnityEngine;

namespace Core.Gameplay.Entity.Subsystem
{
    //
    // Pure query service: answers "who should I target for this attack?"
    // Owns the physics overlap filter and line-of-sight check.
    // Holds no persistent target state — that remains the brain's responsibility.
    //
    public class EntityTargetingSubsystem : BaseSubsystem
    {
        [Header("Line of Sight")]
        [SerializeField] private LayerMask losBlockMask;  // walls, ground, obstacles
        
        private ContactFilter2D _entityFilter;
        private readonly List<Collider2D> _overlapResults = new();
        private readonly List<EntityController> _candidateBuffer = new();

        //
        //  Lifecycle
        //
 
        protected override void OnInitialize()
        {
            _entityFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask    = Controller.Stats.combatEntityLayers,
                useTriggers  = true
            };
        }
 
        // No OnUpdate / OnFixedUpdate — this is a stateless query service.

        
        //
        //  Targeting API
        //
        #region Targeting API
        
        // Find possible combat entities
        // Relationship is resolved by targeting strategy
        public EntityController ResolveAttackTarget(
            AttackData attack,
            bool requireLos = false)
        {
            return attack.targetType switch
            {
                CombatTargetType.Self    => ResolveSelf(),
                CombatTargetType.Enemies => FindBestEnemy(attack.range, attack.targetSelectionMode, requireLos),
                CombatTargetType.Allies  => FindBestAlly(attack.range, attack.targetSelectionMode, attack.targetRequirement),
                CombatTargetType.Any     => FindBestAny(attack.range, attack.targetSelectionMode),
                _                        => null
            };
        }
        
        // Finds the best enemy within radius.
        // Optionally enforces a line-of-sight check.
        // Used by brains to pick a movement target as well as an attack target
        public EntityController FindBestEnemy(
            float radius,
            TargetSelectionMode selectionMode = TargetSelectionMode.Closest,
            bool requireLos = false)
        {
            GatherCandidates(radius);
            FilterToEnemies(requireLos);
            return SelectBest(_candidateBuffer, selectionMode);
        }

        // Finds the best living ally (excluding self) within radius.
        public EntityController FindBestAlly(
            float radius,
            TargetSelectionMode selectionMode = TargetSelectionMode.Closest,
            TargetRequirement requirement = TargetRequirement.None)
        {
            GatherCandidates(radius);
            FilterToAllies();
            FilterByRequirement(requirement);
            return SelectBest(_candidateBuffer, selectionMode);
        }

        // Finds the best entity of any faction (excluding self) within radius.
        public EntityController FindBestAny(
            float radius,
            TargetSelectionMode selectionMode = TargetSelectionMode.Closest)
        {
            GatherCandidates(radius);
            FilterToAny();
            return SelectBest(_candidateBuffer, selectionMode);
        }

        // Returns true when there is an unobstructed straight line between origin and target.
        public bool HasLineOfSight(Vector2 origin, Vector2 target)
        {
            Vector2 dir  = target - origin;
            float dist = dir.magnitude;
 
            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                dir.normalized,
                dist,
                losBlockMask
            );
 
            return hit.collider == null;
        }
        
        #endregion
        
        //
        //  Candidate gathering
        //
        
        #region Candidate gathering
        
        // Fills _candidateBuffer with all living EntityController
        // components found within radius of this entity.
        // Self is never included.
        private void GatherCandidates(float radius)
        {
            _overlapResults.Clear();
            _candidateBuffer.Clear();
 
            Physics2D.OverlapCircle(
                Controller.transform.position,
                radius,
                _entityFilter,
                _overlapResults
            );
 
            foreach (var col in _overlapResults)
            {
                if (!col.TryGetComponent(out EntityController candidate))
                    continue;
 
                if (candidate == Controller)
                    continue;
 
                if (candidate.IsDead)
                    continue;
                
                if (candidate.GetComponent<EntityHealth>() == null)
                    continue;
 
                _candidateBuffer.Add(candidate);
            }
        }
        
        #endregion
        
        //
        //  Filters  (mutate _candidateBuffer in-place)
        //
        #region Filters
        
        private void FilterToEnemies(bool requireLos)
        {
            Vector2 origin = Controller.transform.position;
 
            _candidateBuffer.RemoveAll(c =>
            {
                if (!Controller.IsEnemy(c))
                    return true; // remove non-enemies
 
                if (requireLos && !HasLineOfSight(origin, c.transform.position))
                    return true; // remove LOS-blocked
 
                return false;
            });
        }
 
        private void FilterToAllies()
        {
            _candidateBuffer.RemoveAll(c => !Controller.IsAlly(c));
        }
 
        private void FilterToAny()
        {
            // No extra filtering beyond what GatherCandidates already did.
        }
        
        private void FilterByRequirement(TargetRequirement requirement)
        {
            switch(requirement)
            {
                case TargetRequirement.MissingHealth:

                    _candidateBuffer.RemoveAll(c =>
                    {
                        var health = c.GetComponent<EntityHealth>();

                        return health == null ||
                               health.GetCurrentHealth() >= health.GetCurrentMaxHealth();
                    });

                    break;
            }
        }
        
        #endregion

        //
        //  Selection strategies
        //
        #region Selection Strategies
        
        private EntityController SelectBest(
            List<EntityController> candidates,
            TargetSelectionMode mode)
        {
            if (candidates.Count == 0)
                return null;
 
            if (candidates.Count == 1)
                return candidates[0];
 
            Vector3 origin = Controller.transform.position;
 
            switch (mode)
            {
                case TargetSelectionMode.Closest:
                    return SelectByDistance(candidates, origin, pickFurthest: false);
 
                case TargetSelectionMode.Furthest:
                    return SelectByDistance(candidates, origin, pickFurthest: true);
 
                case TargetSelectionMode.LowestHealth:
                    return SelectByHealth(candidates, pickHighest: false);
 
                case TargetSelectionMode.HighestHealth:
                    return SelectByHealth(candidates, pickHighest: true);
 
                default:
                    return candidates[0];
            }
        }
        
        private static EntityController SelectByDistance(
            List<EntityController> candidates,
            Vector3 origin,
            bool pickFurthest)
        {
            EntityController best = null;
            float bestSq = pickFurthest ? float.MinValue : float.MaxValue;
 
            foreach (var c in candidates)
            {
                float sq = (c.transform.position - origin).sqrMagnitude;
 
                bool isBetter = pickFurthest ? sq > bestSq : sq < bestSq;
                if (!isBetter)
                    continue;
 
                bestSq = sq;
                best = c;
            }
 
            return best;
        }
 
        private static EntityController SelectByHealth(
            List<EntityController> candidates,
            bool pickHighest)
        {
            EntityController best = null;
            int bestValue = pickHighest ? int.MinValue : int.MaxValue;
 
            foreach (var c in candidates)
            {
                var health = c.GetComponent<EntityHealth>();
                if (health == null)
                    continue;
 
                int current = health.GetCurrentHealth();
 
                bool isBetter = pickHighest ? current > bestValue : current < bestValue;
                if (!isBetter)
                    continue;
 
                bestValue = current;
                best = c;
            }
 
            return best;
        }
        
        #endregion
        
        //
        //  Self resolution
        //
 
        private EntityController ResolveSelf()
        {
            // Self-target is only valid if the entity is alive.
            return Controller.IsDead ? null : Controller;
        }
        
        //
        //  Editor
        //
 
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Nothing to draw at the subsystem level;
            // brains draw their own radii with the stats values they know.
        }
#endif

        
    }
}
