using Core.Enum;
using Core.Gameplay.Combat;
using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Core.Gameplay.Entity.Subsystem
{
    [RequireComponent(typeof(EntityModifierSubsystem))]
    public abstract class EntityMovement: BaseSubsystem
    {
        [SerializeField]
        private AnimationCurve knockbackVelocityCurve =
            AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        
        protected Rigidbody2D Rb;
        protected Collider2D Col;
        public Bounds Bounds => Col.bounds;
        
        // Is the entity currently sitting on a surface?
        public bool IsGrounded { get; protected set; }
        public bool IsKnockbackActive => _knockbackActive;
        
        private EntityModifierSubsystem _modifiers;
        private BaseEntityStats _stats;
        
        private Vector2 _locomotionVelocity;

        private bool _knockbackActive;
        private Vector2 _knockbackInitialVelocity;
        private float _knockbackElapsed;
        private float _knockbackDuration;
        
        protected void Awake()
        {
            Rb = GetComponent<Rigidbody2D>();
            Col = GetComponent<Collider2D>();
        }

        protected override void OnInitialize()
        {
            Rb.simulated = true;
            Rb.freezeRotation = true;
            Rb.gravityScale = 1f;
            
            // ensure sync
            Rb.position = transform.position;
            Rb.linearVelocity = Vector2.zero;
            Rb.angularVelocity = 0f;

            Col.enabled = true;
            
            if (Controller.Stats)
                _stats = Controller.Stats;
            else
                Debug.LogError("[EntityAttackSubsystem] requires Stats!");
            
            _modifiers = GetComponent<EntityModifierSubsystem>();
            
            _locomotionVelocity = Vector2.zero;
            ClearKnockback();
        }

        protected override void OnDeinitialize()
        {
            Rb.simulated = false;
            CancelAllMotion();
            Col.enabled = false;
        }
        
        protected override void OnFixedUpdate()
        {
            if (!CanRbMove)
                return;

            Vector2 finalVelocity = IsKnockbackActive
                ? GetCurrentKnockbackVelocity()
                : _locomotionVelocity;

            Rb.linearVelocity = ConstrainFinalVelocity(finalVelocity);

            TickKnockback();
        }
        
        #region Movement API
        
        public abstract void MoveTo(Vector2 target);

        public virtual void Jump() { }

        public virtual void StopJump() { }

        public virtual void UpdateJumpState() { }

        public void ApplyKnockback(Vector2 velocity, float duration)
        {
            if (!CanRbMove)
                return;

            velocity = ConstrainKnockbackVelocity(velocity);

            if (velocity.sqrMagnitude <= 0.0001f)
                return;

            _locomotionVelocity = Vector2.zero;

            _knockbackInitialVelocity = velocity;
            _knockbackDuration = Mathf.Max(duration, Time.fixedDeltaTime);
            _knockbackElapsed = 0f;
            _knockbackActive = true;
        }
        
        protected void SetLocomotionVelocity(Vector2 velocity)
        {
            _locomotionVelocity = ConstrainLocomotionVelocity(velocity);
        }

        protected float GetEffectiveMoveSpeed()
        {
            if (_modifiers == null)
                return _stats.moveSpeed;

            var resolvedMovement = AttackStatResolver.Resolve(
                baseValue: _stats.moveSpeed,
                attack: null,           // no specific attack context
                statType: AttackStatType.MoveSpeed,
                scope: ModifierScope.All,
                hitTypes: HitTypes.None,
                modifiers: _modifiers.AttackStatModifiers  // see note
            );
            
            return resolvedMovement;
        }

        public virtual void Stop()
        {
            Rb.linearVelocity = Vector2.zero;
            
            if (Controller.IsDead) return;
            
            if (Controller.Animator)
            {
                Controller.Animator.SetFloat("velocityX", 0f);
                Controller.Animator.SetFloat("velocityY", 0f);
                Controller.Animator.SetBool("isMoving", false);
            }
        }
        
        public void CancelAllMotion()
        {
            _locomotionVelocity = Vector2.zero;
            ClearKnockback();

            if (Rb)
            {
                Rb.linearVelocity = Vector2.zero;
                Rb.angularVelocity = 0f;
            }
        }
        
        public void Teleport(Vector3 position)
        {
            Rb.position = position;
            CancelAllMotion();
        }
        
        public virtual void OnFootstep(){ }

        protected bool CanRbMove => Rb.simulated;
        
        #endregion
        
        #region Knockback internals

        private Vector2 GetCurrentKnockbackVelocity()
        {
            if (!_knockbackActive)
                return Vector2.zero;

            float t = Mathf.Clamp01(_knockbackElapsed / _knockbackDuration);
            float multiplier = knockbackVelocityCurve.Evaluate(t);

            return _knockbackInitialVelocity * multiplier;
        }

        private void TickKnockback()
        {
            if (!_knockbackActive)
                return;

            _knockbackElapsed += Time.fixedDeltaTime;

            if (_knockbackElapsed >= _knockbackDuration)
                ClearKnockback();
        }

        private void ClearKnockback()
        {
            _knockbackActive = false;
            _knockbackInitialVelocity = Vector2.zero;
            _knockbackElapsed = 0f;
            _knockbackDuration = 0f;
        }

        protected virtual Vector2 ConstrainLocomotionVelocity(Vector2 velocity)
        {
            return velocity;
        }

        protected virtual Vector2 ConstrainKnockbackVelocity(Vector2 velocity)
        {
            return velocity;
        }

        protected virtual Vector2 ConstrainFinalVelocity(Vector2 velocity)
        {
            return velocity;
        }
        
        /*
            Constrains:
            
            LaneMovement:
                X only, Y locked.

            TopDownMovement:
                X/Y allowed, no gravity.

            PlatformerMovement:
                X knockback allowed, Y knockback allowed, but gravity remains active.

            FlyingMovement:
                X/Y allowed, maybe no constraints.

            RootMotionMovement:
                maybe ignores velocity entirely and consumes knockback differently.
         */

        #endregion
    }
}