using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Attack;
using Core.Gameplay.Entity.Subsystem;
using Core.Gameplay.Entity.Tags;
using Game.Entity.Enemy.Stats;
using UnityEngine;

namespace Game.Entity.Enemy.Subsystem
{
    [RequireComponent(typeof(EntityMovement))]
    [RequireComponent(typeof(EntityPresentationSubsystem))]
    [RequireComponent(typeof(EntityAttackSubsystem))]
    [RequireComponent(typeof(EntityAttackLoadout))]
    public sealed class EnemyAiBrainSubsystem : EntityBrainSubsystem
    {
        [Header("Line of Sight")]
        [SerializeField] private LayerMask losBlockMask;  // walls, ground, obstacles
        
        private EntityHealth _health;
        private EntityMovement _movement;
        private EntityAttackSubsystem _attackSubsystem;
        private EntityAttackLoadout _attackLoadout;
        private EntityPresentationSubsystem _presentationSubsystem;
        
        private EnemyStats _stats;
        
        private float _nextActionTime;
        private float _nextScanTime;
        
        protected override void OnInitialize()
        {
            _attackLoadout = GetComponent<EntityAttackLoadout>();
            _movement = GetComponent<EntityMovement>();
            _attackSubsystem = GetComponent<EntityAttackSubsystem>();
            _presentationSubsystem = GetComponent<EntityPresentationSubsystem>();
            _health = Controller.GetComponent<EntityHealth>();
            
            // Adjust to the ideal position for facing direction - when sprite is drawn to the left - given spawn position (P ------- E).
            if (Controller.Stats.faction == Faction.Enemy) // ALWAYS TRUE HERE
                _presentationSubsystem.SetFacing(FacingDirection.Left, force: true);
            
            if (Controller.Stats is EnemyStats enemyStats)
                _stats = enemyStats;
            else
                Debug.LogError("[EnemyAI] requires EnemyStats!");
            
            _entityFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = _stats.combatEntityLayers,
                useTriggers = true
            };
            
            _health.OnDamageTaken += OnDamageTaken;
        }
        
        protected override void OnDeinitialize()
        {
            controlEnabled = false;
            StopAllCoroutines();

            ClearTarget();
            
            _health.OnDamageTaken -= OnDamageTaken;
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

            if (tag == _stats.deadTag || tag == _stats.matchEndedTag)
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
            
            if (!HasAnyPossibleActionTarget())
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
            if (!controlEnabled || Controller.IsStunned || !HasAnyPossibleActionTarget()) 
            {
                _movement.Stop();
                return; // <--- THIS blocks attack/move before spawn finished!
            }
            
            if (_attackSubsystem.IsAttackInProgress)
            {
                _movement.Stop();
                return;
            }
            
            // Mental model:
            // a. Can I attack right now from where I am?
            // b. If not, is there a ready attack I could reach by moving?
            // c. Otherwise, no ready attacks at all -> idle / hold / wait for cooldowns
            
            float distance = HasTarget
                ? Vector2.Distance(
                    transform.position,
                    CurrentTarget.transform.position)
                : float.MaxValue;
            // No enemy target = dont chase
            // Ally exists = still try attacks
            
            // --- ATTACK INTENT ---
            // 1. Attack immediately if possible
            if (Time.time >= _nextActionTime)
            {
                if (TryExecuteAnyReadyAttack())
                    return;
            }
            
            // --- MOVEMENT INTENT ---
            // 2. Move if there exists a READY attack we could reach
            if (HasReadyAttackOutOfRange())
            {
                FaceTarget(CurrentTarget);
                
                _movement.MoveTo(CurrentTarget.transform.position);
                
                return;
            }
            
            // 3. Otherwise, wait (we are in range, but all attacks are on cooldown)
            _movement.Stop();
        }
        
        private void OnDamageTaken(CombatPayload payload)
        {
            //Debug.Log("[EnemyAIBrainSubsystem] Enemy has taken damage from " + payload.source.controller);
            if (payload.source.sourceEntity != null)
                SetTarget(payload.source.sourceEntity);
        }
        
        #region Movement
        
        private void HandleMovement()
        {
            _movement.MoveTo(TargetPosition);
        }
        
        #endregion
        
        
        #region Attack Selection
        
        private bool TryExecuteAnyReadyAttack()
        {
            foreach (var attack in _attackLoadout.Attacks)
            {
                // Attack is on cooldown, skip.
                if (!_attackSubsystem.CanExecute(attack))
                    continue;

                EntityController target = ResolveAttackTarget(attack);
                
                if(attack.combatActionType == CombatAction.Heal)
                    Debug.Log(
                        $"Trying {attack.name}, target={target}"
                    );

                if (!target)
                    continue;
                
                if (attack.targetType != CombatTargetType.Self)
                {
                    float distance =
                        Vector2.Distance(
                            transform.position,
                            target.transform.position);

                    // Attack isn't in range, skip.
                    if (distance > attack.range)
                        continue;
                }
                
                // Ensure facing is correct before attack
                Vector2 attackDir =
                    (target.transform.position - transform.position).normalized;

                FaceTarget(target);

                bool executed = _attackSubsystem.TryExecute(
                    attack,
                    new AttackContext
                    {
                        Target = target,
                        Direction = attackDir
                    });

                if (executed)
                {
                    _movement.Stop();
                    _nextActionTime = Time.time + _stats.globalCooldown;
                    return true;
                }
            }

            return false;
        }
        
        private bool HasReadyAttackOutOfRange()
        {
            foreach (var attack in _attackLoadout.Attacks)
            {
                if (!_attackSubsystem.CanExecute(attack))
                    continue;

                EntityController target =
                    ResolveAttackTarget(attack);

                if (!target)
                    continue;
                
                if (attack.targetType == CombatTargetType.Self)
                    return false;
                
                float distance =
                    Vector2.Distance(
                        transform.position,
                        target.transform.position);
                
                if (distance > attack.range)
                    return true;
            }

            return false;
        }
        
        #endregion
        
        // TODO: This could be added to a TargetingSubsystem.
        // TryAcquireTarget() becomes a configurable acquisition strategy, something like: ClosestEnemyTargetingStrategy (similar to what we've done for ProjectileMovement/Impact).
        // Also probably extend with FindClosestEnemy(), FindClosestAlly(), FindTargetsInRadius().
        #region Targeting
        
        private readonly List<Collider2D> _overlapResults = new();
        private ContactFilter2D _entityFilter;
        private float _positionOffset;
        
        //
        // Find possible combat entities
        // Relationship is resolved by targeting strategy
        //
        private EntityController ResolveAttackTarget(
            AttackData attack)
        {
            switch (attack.targetType)
            {
                case CombatTargetType.Self:
                    return Controller;

                case CombatTargetType.Enemies:
                    return CurrentTarget;

                case CombatTargetType.Allies:
                    return FindClosestAlly();

                case CombatTargetType.Any:
                    return FindClosestEntity();

                default:
                    return null;
            }
        }
        
        /*
         * TODO: Later we might want something like:
         * TargetingMode:
            - Aggressive
            - Defensive
            - Support
            - ProtectAlly
            - Selfish
         */
        private void TryAcquireTarget()
        {
            _overlapResults.Clear();
            
            Physics2D.OverlapCircle(
                Controller.transform.position,
                _stats.aggroRadius,
                _entityFilter,
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

                if (!Controller.IsEnemy(candidate))
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
            if (CurrentTarget == null || !CurrentTarget.gameObject || CurrentTarget.IsDead)
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
        
        private bool HasTarget =>
            CurrentTarget != null &&
            CurrentTarget.gameObject != null &&
            !CurrentTarget.IsDead;
        
        // TODO: We keep this for now because movement is enemy-centric, but later we might want a healer brain that override this. 
        private Vector2 TargetPosition =>
            HasTarget 
                ? new Vector2(
                    CurrentTarget.transform.position.x + _positionOffset, 
                    CurrentTarget.transform.position.y) 
                : Vector2.zero;

        private void FaceTarget(EntityController target)
        {
            if (!target)
                return;
            
            _presentationSubsystem.FaceDirection(
                (target.transform.position - transform.position).normalized
            );
        }
        
        // This checks: Should this AI brain continue processing actions this frame?
        private bool HasAnyPossibleActionTarget()
        {
            /* Originally we had:
             
                if(CurrentTarget)
                    return true;

                return FindClosestAlly() != null;
            
            Which covered:

                enemy exists → attack it
                ally exists → heal/buff it

            But it missed:

                no enemy
                no ally
                self-cast ability available
                
            (It was also expensive since we were checking physics every frame in FindClosestAlly)
            */
            
            return HasTarget || HasSelfActionAvailable();
            /* Then we add HasSelfActionAvailable() to the check, which is only doing this:
"               Before giving up and becoming idle, do I have a ready action that 
                does not require another entity?
             */
        }
        
        private bool HasSelfActionAvailable()
        {
            foreach(var attack in _attackLoadout.Attacks)
            {
                if (!_attackSubsystem.CanExecute(attack))
                    continue;

                if (attack.targetType == CombatTargetType.Self)
                    return true;
            }

            return false;
        }
        
        private EntityController FindClosestAlly()
        {
            _overlapResults.Clear();

            Physics2D.OverlapCircle(
                transform.position,
                _stats.aggroRadius,
                _entityFilter,
                _overlapResults);

            EntityController best = null;

            float bestDistance = float.MaxValue;

            foreach(var col in _overlapResults)
            {
                if(!col.TryGetComponent(out EntityController candidate))
                    continue;

                if(candidate == Controller)
                    continue;
                
                if(candidate.IsDead)
                    continue;

                if(!Controller.IsAlly(candidate))
                    continue;

                float distance =
                    (candidate.transform.position -
                     transform.position)
                    .sqrMagnitude;

                if(distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }

            Debug.Log(
                $"[{name}] Closest ally = {best}"
            );

            return best;
        }
        
        private EntityController FindClosestEntity()
        {
            _overlapResults.Clear();

            Physics2D.OverlapCircle(
                transform.position,
                _stats.aggroRadius,
                _entityFilter,
                _overlapResults);

            EntityController best = null;

            float bestDistance = float.MaxValue;
            
            foreach(var col in _overlapResults)
            {
                if(!col.TryGetComponent(out EntityController candidate))
                    continue;
                
                if(candidate == Controller)
                    continue;
                
                if(candidate.IsDead)
                    continue;

                float distance =
                    (candidate.transform.position -
                     transform.position)
                    .sqrMagnitude;

                if(distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }
            
            return best;
        }

        #endregion
        
                
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
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
