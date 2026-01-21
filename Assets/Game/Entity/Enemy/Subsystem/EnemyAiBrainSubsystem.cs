using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Attack;
using Core.Gameplay.Entity.Subsystem;
using Core.Gameplay.Entity.Tags;
using Enemy;
using UnityEngine;

namespace Game.Entity.Enemy.Subsystem
{
    [RequireComponent(typeof(EntityMovement))]
    [RequireComponent(typeof(EntityAttackSubsystem))]
    [RequireComponent(typeof(EntityAttackLoadout))]
    public class EnemyAiBrainSubsystem : EntityBrainSubsystem
    {
        [Header("Line of Sight")]
        [SerializeField] private LayerMask losBlockMask;  // walls, ground, obstacles
        
        private EntityHealth _health;
        private EntityMovement _movement;
        private EntityAttackSubsystem _attackSubsystem;
        private EntityAttackLoadout _attackLoadout;
        
        private EnemyStats _stats;
        
        private float _nextActionTime;
        private float _nextScanTime;
        
        protected override void OnInitialize()
        {
            _attackLoadout = GetComponent<EntityAttackLoadout>();
            _movement = GetComponent<EntityMovement>();
            _attackSubsystem = GetComponent<EntityAttackSubsystem>();
            _health = Controller.GetComponent<EntityHealth>();
            
            if (Controller.Stats is EnemyStats enemyStats)
                _stats = enemyStats;
            else
                Debug.LogError("[EnemyAI] requires EnemyStats!");
            
            _targetFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = _stats.targetEntityLayer,
                useTriggers = true
            };
            
            _health.DamageTaken += OnDamageTaken;
        }
        
        protected override void OnDeinitialize()
        {
            controlEnabled = false;
            StopAllCoroutines();

            ClearTarget();
            
            _health.DamageTaken -= OnDamageTaken;
            _health = null;
        }

        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == _stats.spawnFinishedTag)
            {
                controlEnabled = true;
                _nextActionTime = Time.time + _stats.thinkingTime;
                //_nextScanTime = Time.time; // allow immediate scan
            }

            if (tag == _stats.deadTag)
            {
                controlEnabled = false;
                
                StopAllCoroutines();
                _movement.Stop();
            }
        }

        protected override void OnUpdate()
        {
            if (!controlEnabled)
                return;
            
            if (!HasTarget)
            {
                if (Time.time >= _nextScanTime)
                {
                    _nextScanTime = Time.time + _stats.thinkingTime;
                    TryAcquireTarget();
                }
                return;
            }
            
            ValidateCurrentTarget();
        }
        
        protected override void OnFixedUpdate()
        {
            if (!controlEnabled || Controller.IsStunned || !HasTarget) 
            {
                _movement.Stop();
                return; // <--- THIS blocks attack/move before spawn finished!
            }
            
            bool hasAttackInRange = HasAnyValidAttackInRange();
            
            // --- ATTACK INTENT ---
            if (Time.time >= _nextActionTime)
            {
                float distance =
                    Vector2.Distance(transform.position, CurrentTarget.transform.position);

                foreach (var attack in _attackLoadout.Attacks) // Or later: foreach (var attack in GetCandidateAttacks(CurrentTarget))
                {
                    // High-level AI intent check only
                    if (!IsAttackAppropriate(attack, distance))
                        continue;

                    // Ensure facing is correct before attack
                    _movement.CheckDirectionChange(CurrentTarget);  // Change sprite direction if needed.
                    // Later on we might want to introduce attack wind-up time, i.e. lock direction on attack start.
                
                    Vector2 direction = 
                        (CurrentTarget.transform.position - transform.position).normalized;
                    
                    bool attackExecuted = _attackSubsystem.TryExecute(
                        attack,
                        new AttackContext
                        {
                            Target = CurrentTarget,
                            Direction = direction
                        }
                    );

                    if (attackExecuted)
                    {
                        _movement.Stop();
                        _nextActionTime = Time.time + _stats.globalCooldown;
                        return; // Attack consumed → no movement this frame
                    }
                }
            }
            
            // --- MOVEMENT ---
            if (hasAttackInRange)
            {
                // We are in range but waiting for cooldown → HOLD POSITION
                _movement.Stop();
                return;
            }
            
            HandleMovement();
        }
        
        private void OnDamageTaken(DamagePayload payload)
        {
            //Debug.Log("[EnemyAIBrainSubsystem] Enemy has taken damage from " + payload.source.controller);
            if (payload.source.controller != null)
                SetTarget(payload.source.controller);
        }
        
        #region Movement
        
        private void HandleMovement()
        {
            var shouldMove =
                    Vector2.Distance(transform.position, TargetPosition) > _stats.preferredDistance;

            if (!shouldMove)
            {
                _movement.Stop();
                return;
            }

            _movement.MoveTo(TargetPosition);
        }
        
        #endregion
        
        
        #region Attack Selection
        
        private bool HasAnyValidAttackInRange()
        {
            float distance =
                Vector2.Distance(transform.position, CurrentTarget.transform.position);

            foreach (var attack in _attackLoadout.Attacks)
            {
                if (IsAttackAppropriate(attack, distance))
                    return true;
            }

            return false;
        }
        
        private bool IsAttackAppropriate(AttackData attack, float distance)
        {
            // High-level AI intent check ONLY
            // Exact range validation happens inside AttackSubsystem
            return distance <= attack.range;
        }
        
        #endregion
        
        // TODO: This could be added to a TargetingSubsystem.
        #region Targeting
        
        private readonly List<Collider2D> _overlapResults = new();
        private ContactFilter2D _targetFilter;
        private float _positionOffset;
        
        //
        //  Enemy-to-Player checks
        //
        private void TryAcquireTarget()
        {
            _overlapResults.Clear();
            
            Physics2D.OverlapCircle(
                Controller.transform.position,
                _stats.aggroRadius,
                _targetFilter,
                _overlapResults
            );

            EntityController bestTarget = null;
            float bestDistSq = float.MaxValue;
            Vector2 origin = Controller.transform.position;
            
            foreach (var col in _overlapResults)
            {
                if (!col.TryGetComponent(out EntityController candidate))
                    continue;

                if (candidate.IsDead)
                    continue;

                if (candidate.Stats.faction != Faction.Player)  // TODO: This will ignore Ally layer. Should this be any opponent Layer?
                    continue;

                Vector2 toTarget = candidate.transform.position - (Vector3)origin;
                float distSq = toTarget.sqrMagnitude;
                
                // Early distance reject
                if (distSq >= bestDistSq)
                    continue;

                // Line-of-sight check
                if (!HasLineOfSight(origin, candidate.transform.position))
                    continue;

                bestDistSq = distSq;
                bestTarget = candidate;
            }

            if (bestTarget)
                SetTarget(bestTarget);
        }
        
        private bool HasLineOfSight(Vector2 origin, Vector2 target)
        {
            Vector2 dir = target - origin;
            float dist = dir.magnitude;

            RaycastHit2D hit = Physics2D.Raycast(
                origin,
                dir.normalized,
                dist,
                losBlockMask
            );

            // If we hit something, LOS is blocked
            return hit.collider == null;
        }
        
        private void SetTarget(EntityController target)
        {
            if (target == null || target.IsDead)
                return;
            
            CurrentTarget = target;
            
            // Random X offset (always positive or always negative based on spawn position)
            float direction = 
                transform.position.x > CurrentTarget.transform.position.x ? 1f : -1f;
            
            _positionOffset = Random.Range(0.5f, 1f) * direction; // Will space them out based on which side they spawn
        }
        
        private void ClearTarget()
        {
            CurrentTarget = null;
        }
        
        private void ValidateCurrentTarget()
        {
            if (CurrentTarget == null || CurrentTarget.IsDead)
            {
                ClearTarget();
                return;
            }

            float distSq = 
                (CurrentTarget.transform.position - Controller.transform.position).sqrMagnitude;

            float loseAggroRadius = _stats.aggroRadius + _stats.aggroTolerance;
            
            if (distSq > loseAggroRadius * loseAggroRadius)
                ClearTarget(); // Clearing target because out of range.
        }
        
        private bool HasTarget => CurrentTarget && !CurrentTarget.IsDead;
        private Vector2 TargetPosition =>
            HasTarget 
                ? new Vector2(
                    CurrentTarget.transform.position.x + _positionOffset, 
                    CurrentTarget.transform.position.y) 
                : Vector2.zero;

        #endregion
        
                
#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (!_stats)
                return;
            
            Gizmos.color = Color.yellow;

            // Draw agroo radius
            Gizmos.DrawWireSphere(transform.position, _stats.aggroRadius);
        }
#endif
    }
}
