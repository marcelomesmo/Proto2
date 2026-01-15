using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Attack;
using Core.Gameplay.Entity.Subsystem;
using Core.Gameplay.Entity.Tags;
using UnityEngine;
using UnityEngine.Serialization;

namespace Enemy
{
    [RequireComponent(typeof(EntityMovement))]
    [RequireComponent(typeof(EntityAttackSubsystem))]
    [RequireComponent(typeof(EntityAttackLoadout))]
    public class EnemyAiBrainSubsystem : EntityBrainSubsystem
    {
        [Header("Line of Sight")]
        [SerializeField] private LayerMask losBlockMask;  // walls, ground, obstacles
        
        private EntityMovement _movement;
        private EntityAttackSubsystem _attackSubsystem;
        private EntityHealth _health;
        private EntityAttackLoadout _attackLoadout;
        
        private EnemyStats _stats;
        
        private float _nextActionTime;
        private float _nextScanTime;
        
        // internal Timers for jump/dash
        private float _jumpDelayTimer = 0f;
        private float _jumpCooldownTimer = 0f;
        private float _dashDelayTimer = 0f;
        private float _dashCooldownTimer = 0f;
        
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
            
            _health.DamageTaken += OnDamageTaken;
            
            ResetTimers();
        }
        
        protected override void OnDeinitialize()
        {
            controlEnabled = false;
            StopAllCoroutines();

            ClearTarget();
            
            _health.DamageTaken -= OnDamageTaken;
            _health = null;

            ResetTimers();
        }

        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == _stats.spawnFinishedTag)
            {
                controlEnabled = true;
                _nextActionTime = Time.time + _stats.thinkingTime;
            }

            if (tag == _stats.deadTag)
            {
                controlEnabled = false;
                
                StopAllCoroutines();
                _movement.Stop();
                
                ResetTimers();
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
            HandleMovement();

            // --- TIMERS ---
            float dt = Time.fixedDeltaTime;

            if (_jumpDelayTimer > 0f) _jumpDelayTimer -= dt;
            if (_jumpCooldownTimer > 0f) _jumpCooldownTimer -= dt;

            if (_dashDelayTimer > 0f) _dashDelayTimer -= dt;
            if (_dashCooldownTimer > 0f) _dashCooldownTimer -= dt;
        }
        
        private void OnDamageTaken(DamagePayload payload)
        {
            //Debug.Log("[EnemyAIBrainSubsystem] Enemy has taken damage from " + payload.source.controller);
            if (payload.source.controller != null)
                SetTarget(payload.source.controller);
        }
        
        private void ResetTimers()
        {
            // Reset timers to prevent any further jumps/dashes
            _jumpDelayTimer = 0f;
            _jumpCooldownTimer = 0f;
            _dashDelayTimer = 0f;
            _dashCooldownTimer = 0f;
        }
        
        #region Movement
        
        private void HandleMovement()
        {
            bool shouldMove;

            if (_stats.movementType == EnemyMovementType.Flyer)
                shouldMove =
                    Mathf.Abs(transform.position.x - TargetPosition.x) > _stats.preferredDistance ||
                    Mathf.Abs(transform.position.y - TargetPosition.y) > _stats.flyHoverHeight;
            else
                shouldMove =
                    Vector2.Distance(transform.position, TargetPosition) > _stats.preferredDistance;

            if (!shouldMove)
            {
                _movement.Stop();
                return;
            }

            switch (_stats.movementType)
            {
                case EnemyMovementType.Jumper:
                    HandleJump();
                    break;

                case EnemyMovementType.Dasher:
                    HandleDash();
                    break;

                default:
                    _movement.MoveTo(TargetPosition);
                    break;
            }
        }
        
        private void HandleJump()
        {
            if (_jumpDelayTimer > 0f || _jumpCooldownTimer > 0f) return;

            _jumpDelayTimer = 0.2f; // pre-jump delay
            _jumpCooldownTimer = _stats.thinkingTime;

            _movement.MoveTo(TargetPosition);
        }
        
        private void HandleDash()
        {
            if (_dashDelayTimer > 0f || _dashCooldownTimer > 0f) return;

            _dashDelayTimer = _stats.dashDelay;
            _dashCooldownTimer = _stats.dashCooldown;

            _movement.MoveTo(TargetPosition);
        }
        
        #endregion
        
        #region Attack Selection
        private IEnumerable<AttackData> GetCandidateAttacks(EntityController target)
        {
            float distance =
                Vector2.Distance(transform.position, target.transform.position);

            foreach (var attack in _attackLoadout.Attacks)
            {
                if (IsAttackAppropriate(attack, distance))
                    yield return attack;
            }
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
        
        private readonly Collider2D[] _overlapResults = new Collider2D[8];
        private float _positionOffset;
        
        //
        //  Enemy-to-Player checks
        //
        private void TryAcquireTarget()
        {
            int count = Physics2D.OverlapCircleNonAlloc(
                Controller.transform.position,
                _stats.aggroRadius,
                _overlapResults,
                _stats.entityMask
            );

            EntityController bestTarget = null;
            float bestDistSq = float.MaxValue;
            Vector2 origin = Controller.transform.position;
            
            for (int i = 0; i < count; i++)
            {
                if (!_overlapResults[i].TryGetComponent(out EntityController candidate))
                    continue;

                if (candidate.IsDead)
                    continue;

                if (candidate.Stats.faction != Faction.Player)  // TODO: This should be any opponentLayer.
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

            if (bestTarget != null)
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
    }
}
