using Core.Enum;
using Core.Gameplay.Combat;
using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Core.Gameplay.Entity.Subsystem
{
    [RequireComponent(typeof(EntityModifierSubsystem))]
    public abstract class EntityMovement: BaseSubsystem
    {
        protected Rigidbody2D Rb;
        protected Collider2D Col;
        public Bounds Bounds => Col.bounds;
        
        // Is the entity currently sitting on a surface?
        public bool IsGrounded { get; protected set; }
        
        private EntityModifierSubsystem _modifiers;
        private BaseEntityStats _stats;
        
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
        }

        protected override void OnDeinitialize()
        {
            Rb.simulated = false;
            Rb.linearVelocity = Vector2.zero;
            Rb.angularVelocity = 0f;

            Col.enabled = false;
        }
        
        #region Movement API
        
        public abstract void MoveTo(Vector2 target);

        public virtual void Jump() { }

        public virtual void StopJump() { }

        public virtual void UpdateJumpState() { }

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
        
        public void Teleport(Vector3 position)
        {
            Rb.position = position;
            //appliedVelocity (movespeed?) *= 0;
            Rb.linearVelocity *= 0;
        }
        
        public virtual void OnFootstep(){ }

        protected bool CanRbMove => Rb.simulated;
        
        #endregion
    }
}