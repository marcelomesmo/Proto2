using Entity;
using Entity.Tags;
using Gameplay.Projectile;
using UnityEngine;

namespace Enemy
{
    
    [RequireComponent(typeof(EntityMovement))]
    [RequireComponent(typeof(EnemyAttackSubsystem))]
    public class EnemyAiBrainSubsystem : EntityBrainSubsystem
    {
        private EntityMovement _movement;
        private EnemyAttackSubsystem _attackSubsystem;
        private EntityHealth _health;
        
        private float _lastAttackTime = -Mathf.Infinity;
        // internal Timers for jump/dash
        private float _jumpDelayTimer = 0f;
        private float _jumpCooldownTimer = 0f;
        private float _dashDelayTimer = 0f;
        private float _dashCooldownTimer = 0f;

        private EnemyStats _stats;
        
        protected override void OnInitialize()
        {
            _movement = GetComponent<EntityMovement>();
            _attackSubsystem = GetComponent<EnemyAttackSubsystem>();
            _health = Controller.GetComponent<EntityHealth>();

            _health.DamageTaken += OnDamageTaken;
            
            // Reset timers
            _jumpDelayTimer = 0f;
            _jumpCooldownTimer = 0f;
            _dashDelayTimer = 0f;
            _dashCooldownTimer = 0f;
            
            if (Controller.Stats is EnemyStats enemyStats)
                _stats = enemyStats;
            else
                Debug.LogError("[EnemyAI] requires EnemyStats!");
        }
        
        protected override void OnDeinitialize()
        {
            controlEnabled = false;
            
            StopAllCoroutines(); // just in case
            // _movement.Stop(); ?
            _lastAttackTime = -Mathf.Infinity;
            
            _jumpDelayTimer = 0f;
            _jumpCooldownTimer = 0f;
            _dashDelayTimer = 0f;
            _dashCooldownTimer = 0f;

            ClearTarget();
            
            _health.DamageTaken -= OnDamageTaken;
            _health = null;
        }
        
        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == _stats.spawnFinishedTag)
                controlEnabled = true;

            if (tag == _stats.deadTag)
            {
                StopAllCoroutines();
                _movement.Stop();
                
                // Reset timers to prevent any further jumps/dashes
                _jumpDelayTimer = 0f;
                _jumpCooldownTimer = 0f;
                _dashDelayTimer = 0f;
                _dashCooldownTimer = 0f;
            }
        }

        protected override void OnUpdate()
        {
            if (HasTarget)
            {
                ValidateCurrentTarget();
                return;
            }
            
            if (Time.time >= _nextScanTime) // TODO: Move this to Stats.ThinkTime.
            {
                _nextScanTime = Time.time + _stats.thinkingTime;
                TryAcquireTarget();
            }
        }
        
        protected override void OnFixedUpdate()
        {
            if (!controlEnabled) 
            {
                _movement.Stop();
                return; // <--- THIS blocks attack/move before spawn finished!
            }
            
            // Attack logic first
            if (AI_CanAttack())
            {
                _movement.Stop();
                _movement.CheckDirectionChange(CurrentTarget);
                _attackSubsystem.Attack(CurrentTarget);
                _lastAttackTime = Time.time;
                return; // Do not move if attacking     // TODO: Rework this, as it will only return during the frame it is attacking, should use a state machine to check attack and movement sequence for more trusted source.
            }
            
            // Move depending on movement type
            if (AI_CanMove() && !AI_HasAttackRange())   // Only move if not in attack range.
            {
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
            else
            {
                _movement.Stop();
            }
            
            // --- TIMERS UPDATE ---
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
        
        #region Movement Check
        private bool AI_CanMove()
        {
            if (!HasTarget) return false;  // TODO: Remove this later when adding movement patterns, and add the proper checks.
            
            // Status effect checks
            if (Controller.IsStunned) return false;

            bool inMoveDistance =
                _stats.movementType == EnemyMovementType.Flyer ?
                    Mathf.Abs(transform.position.x - TargetPosition.x) > _stats.preferredDistance ||
                    Mathf.Abs(transform.position.y - TargetPosition.y) > _stats.flyHoverHeight
                    :
                    Vector2.Distance(transform.position, CurrentTarget.transform.position) > _stats.preferredDistance;
            
            return inMoveDistance && controlEnabled;
        }
        #endregion
        
        #region Attack Check
        private bool AI_CanAttack()
        {
            /*if (!HasTarget) return false;
            
            if (!_attackSubsystem) return false;
            
            // Status effect checks
            if (Controller.IsStunned) return false;

            float distance = Vector2.Distance(transform.position, TargetPosition);
            bool inRange = distance <= _stats.attackRange;*/
            bool cooldownReady = (Time.time - _lastAttackTime) >= _stats.attackCooldown;

            return AI_HasAttackRange() && cooldownReady;
        }

        private bool AI_HasAttackRange()
        {
            if (!HasTarget) return false;
            
            if (!_attackSubsystem) return false;
            
            // Status effect checks
            if (Controller.IsStunned) return false;

            float distance = Vector2.Distance(transform.position, TargetPosition);
            bool inRange = distance <= _stats.attackRange;

            return inRange;
        }
        #endregion
        
        #region Jump/Dash Handling
        private void HandleJump()
        {
            if (!HasTarget) return;
            
            if (_jumpDelayTimer > 0f || _jumpCooldownTimer > 0f) return;

            _jumpDelayTimer = 0.2f; // pre-jump delay
            _jumpCooldownTimer = _stats.jumpCooldown;

            _movement.MoveTo(TargetPosition);
        }
        private void HandleDash()
        {
            if (!HasTarget) return;
            
            if (_dashDelayTimer > 0f || _dashCooldownTimer > 0f) return;

            _dashDelayTimer = _stats.dashDelay;
            _dashCooldownTimer = _stats.dashCooldown;

            _movement.MoveTo(TargetPosition);
        }
        #endregion
        
        // TODO: This could be added to a TargetingSubsystem.
        #region Targetting
        
        private float _positionOffset;
        
        public void SetTarget(EntityController target)
        {
            if (target == null || target.IsDead)
                return;
            
            CurrentTarget = target;
            
            // Random X offset (always positive or always negative based on spawn position)
            float direction = transform.position.x > CurrentTarget.transform.position.x ? 1f : -1f;
            _positionOffset = Random.Range(0.5f, 1f) * direction; // Will space them out based on which side they spawn
        }

        public void ClearTarget()
        {
            CurrentTarget = null;
        }
        
        private void ValidateCurrentTarget()
        {
            var target = CurrentTarget;

            if (target == null || target.IsDead)
            {
                ClearTarget();
                return;
            }

            float distSq = (target.transform.position - Controller.transform.position).sqrMagnitude;

            float loseAggroRadius = _stats.aggroRadius + _stats.aggroTolerance;
            
            if (distSq > loseAggroRadius * loseAggroRadius)
            {
                ClearTarget();
                Debug.Log("[EnemyAiBrainSubsystem] Clearing target because out of range.");
            }
        }
        
        //
        //  Enemy-to-Player checks
        //
        private readonly Collider2D[] _overlapResults = new Collider2D[8];
        private float _nextScanTime;
        [SerializeField] private LayerMask losBlockMask; // walls, ground, obstacles
        
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
                Collider2D col = _overlapResults[i];
                
                if (!col.TryGetComponent(out EntityController candidate))
                    continue;

                if (candidate.IsDead)
                    continue;

                if (candidate.Stats.faction != Faction.Player)
                    continue;

                Vector2 targetPos = candidate.transform.position;
                Vector2 dir = targetPos - origin;
                float distSq = dir.sqrMagnitude;
                
                // Early distance reject
                if (distSq >= bestDistSq)
                    continue;

                // Line-of-sight check
                if (!HasLineOfSight(origin, targetPos))
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

        private bool HasTarget => CurrentTarget && !CurrentTarget.IsDead;
        public Vector2 TargetPosition =>
            HasTarget ? new Vector2(CurrentTarget.transform.position.x + _positionOffset, CurrentTarget.transform.position.y) : Vector2.zero;

        #endregion
    }
}
