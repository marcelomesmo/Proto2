using Entity;
using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(EntityMovement))]
    [RequireComponent(typeof(EnemyAttackSubsystem))]
    public class EnemyAI : BaseSubsystem
    {
        private EntityMovement _movement;
        private EnemyAttackSubsystem _attackSubsystem;
        
        /*
            DEPRECATED
         
         public bool controlEnabled = false;
        private float _lastAttackTime = -Mathf.Infinity;
        // internal Timers for jump/dash
        private float _jumpDelayTimer = 0f;
        private float _jumpCooldownTimer = 0f;
        private float _dashDelayTimer = 0f;
        private float _dashCooldownTimer = 0f;
        
        private EnemyStats _stats;
        private EnemyController _controller;
        public Transform Target { get; protected set; }
        
        protected override void OnInitialize()
        {
            _movement = GetComponent<EntityMovement>();
            _attackSubsystem = GetComponent<EnemyAttackSubsystem>();
            
            // Reset timers
            _jumpDelayTimer = 0f;
            _jumpCooldownTimer = 0f;
            _dashDelayTimer = 0f;
            _dashCooldownTimer = 0f;
            
            if (Controller.Stats is EnemyStats enemyStats)
                _stats = enemyStats;
            else
                Debug.LogError("[EnemyAI] requires EnemyStats!");
            
            if (Controller is EnemyController enemyController)
                _controller = enemyController;
            else
                Debug.LogError("[EnemyAI] requires EnemyController!");
            
            Target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        protected override void OnDeinitialize()
        {
            StopAllCoroutines(); // just in case
            controlEnabled = false;
            _lastAttackTime = -Mathf.Infinity;
            
            _jumpDelayTimer = 0f;
            _jumpCooldownTimer = 0f;
            _dashDelayTimer = 0f;
            _dashCooldownTimer = 0f;
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
        
        private void FixedUpdate()
        {
            // Interrupt AI if Dead or no-Target.
            if (Controller.IsDead || !Target) return;
            
            if (!controlEnabled) 
            {
                _movement.Stop();
                return; // <--- THIS blocks attack/move before spawn finished!
            }
            
            // TODO: Refactor this to have a reference to the target (to check if dead, etc).
            Transform target = Target;

            // Attack logic first
            if (AI_CanAttack(target))
            {
                _movement.Stop();
                _attackSubsystem.Attack(target.GetComponentInParent<EntityController>());
                _lastAttackTime = Time.time;
                return; // Do not move if attacking
            }

            // Move depending on movement type
            if (AI_CanMove(target))
            {
                switch (_stats.movementType)
                {
                    case EnemyMovementType.Jumper:
                        HandleJump(target);
                        break;
                    case EnemyMovementType.Dasher:
                        HandleDash(target);
                        break;
                    default:
                        _movement.MoveTo(target.position);
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
        
        #region Movement Check
        
        private bool AI_CanMove(Transform target)
        {
            // Status effect checks
            if (Controller.IsStunned) return false;

            bool inMoveDistance =
                _stats.movementType == EnemyMovementType.Flyer ?
                Mathf.Abs(transform.position.x - target.position.x) > _stats.preferredDistance ||
                Mathf.Abs(transform.position.y - target.position.y) > _stats.flyHoverHeight
                :
                Vector2.Distance(transform.position, target.position) > _stats.preferredDistance;
            
            return inMoveDistance && controlEnabled;
        }
        
        #endregion
        
        #region Attack Check
        private bool AI_CanAttack(Transform target)
        {
            if (!_attackSubsystem) return false;
            // Status effect checks
            if (Controller.IsStunned) return false;

            if (target.GetComponent<EntityController>().IsDead) return false;

            float distance = Vector2.Distance(transform.position, target.position);
            bool inRange = distance <= _stats.attackRange;
            bool cooldownReady = (Time.time - _lastAttackTime) >= _stats.attackCooldown;

            return inRange && cooldownReady;
        }
        #endregion
        
        #region Jump/Dash Handling
        private void HandleJump(Transform target)
        {
            if (_jumpDelayTimer > 0f || _jumpCooldownTimer > 0f) return;

            _jumpDelayTimer = 0.2f; // pre-jump delay
            _jumpCooldownTimer = _stats.jumpCooldown;

            _movement.MoveTo(target.position);
        }
        private void HandleDash(Transform target)
        {
            if (_dashDelayTimer > 0f || _dashCooldownTimer > 0f) return;

            _dashDelayTimer = _stats.dashDelay;
            _dashCooldownTimer = _stats.dashCooldown;

            _movement.MoveTo(target.position);
        }
        #endregion
        
        */
    }
}
