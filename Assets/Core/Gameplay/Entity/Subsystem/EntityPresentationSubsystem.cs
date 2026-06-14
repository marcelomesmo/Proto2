using System;
using Core.Enum;
using Core.Gameplay.Entity.Tags;
using Game.Entity.Presentation;
using UnityEngine;

namespace Core.Gameplay.Entity.Subsystem
{
    public class EntityPresentationSubsystem : BaseSubsystem
    {
        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform[] mirroredAnchors; // cast point, weapon, etc.
        
        [Header("Anchors")]
        [SerializeField] private Transform castAnchor;

        public Transform CastAnchor => castAnchor;
        
        public FacingDirection CurrentFacing { get; private set; }
        public event Action<FacingDirection> OnFacingChanged;
        private bool _facingLocked;
        
        // TODO: if we later want timers in status icons, we add this here and subscribe to the events.
        //private EntityStatusEffectSubsystem _statusEffects;
        private EntityStatusIconController _statusIcons;
        
        protected override void OnInitialize()
        {
            // Explicit initial state — no ambiguity
            SetFacing(FacingDirection.Left, force: true);

            // TODO: maybe later replace with a reference?
            _statusIcons =
                Controller.GetComponentInChildren<EntityStatusIconController>();
            
            // TODO: (see above)
            /*
                _statusEffects =
                    Controller.GetComponent<EntityStatusEffectSubsystem>();

                if (_statusEffects != null)
                {
                    _statusEffects.OnStatusAdded += HandleStatusAdded;
                    _statusEffects.OnStatusRemoved += HandleStatusRemoved;
                    _statusEffects.OnStatusUpdated += HandleStatusUpdated;
                }

                and then in HandleStatusAdded we do:
                    _statusIcons?.Add(effect);
                instead of having in HandleTagAdded the _statusIcons?.Add(tag).
             */
        }

        protected override void HandleTagAdded(GameplayTag tag)
        {
            if(tag == null)
                return;

            _statusIcons?.Add(tag);
            
            if (tag == Controller.Stats.deadTag)
            {
                Controller.Animator.SetBool("isDead", true);
                
                Controller.Animator.ResetTrigger("attack");
                Controller.Animator.ResetTrigger("jump");
                Controller.Animator.ResetTrigger("dash");
                Controller.Animator.ResetTrigger("spawned");
            }
        }

        protected override void HandleTagRemoved(GameplayTag tag)
        {
            _statusIcons?.Remove(tag);
        }
        
        // ---------------------------
        // Animation Events
        // ---------------------------
        
        // Called via animation event at end of Death animation
        public void NotifyDeathAnimationFinished()
        {
            Controller.NotifyDeathAnimationFinished();
        }
       
        public void NotifySpawnAnimationFinished()
        {
            Controller.Animator.SetTrigger("spawned");
            
            Controller.Tags.RemoveTag(Controller.Stats.invulnerableTag); // Force loss of invulnerability
            Controller.Tags.AddTag(Controller.Stats.spawnFinishedTag);
        }
        
        // ------------------
        // Presentation
        // ------------------
        
        public void FaceDirection(Vector2 direction)
        {
#if UNITY_EDITOR
            if (direction.sqrMagnitude < 0.0001f)
            {
                Debug.LogWarning(
                    $"[Facing] Zero direction on {name}",
                    this);
                return;
            }
#endif        
            
            if (_facingLocked)
                return;
            
            if (direction.x > 0.01f)
                SetFacing(FacingDirection.Right);
            else if (direction.x < -0.01f)
                SetFacing(FacingDirection.Left);
        }
        
        public void SetFacing(FacingDirection facing, bool force = false)
        {
            if (!force && facing == CurrentFacing)
                return;

            CurrentFacing = facing;

            // Sprite
            spriteRenderer.flipX = (facing == FacingDirection.Right);

            // Anchors
            foreach (var anchor in mirroredAnchors)
            {
                Vector3 local = anchor.localPosition;
                local.x = Mathf.Abs(local.x) * (int)facing;
                anchor.localPosition = local;
            }
            
            // Rotate Cast Anchor
            if(castAnchor)
                castAnchor.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        facing == FacingDirection.Right ? 0f : 180f);

            OnFacingChanged?.Invoke(facing);
        }

        public void ApplySpriteOverride(Sprite spriteOverride)
        {
            if (!spriteOverride)
                return;
            
            spriteRenderer.sprite = spriteOverride;
        }

        public void ApplyAnimatorOverride(RuntimeAnimatorController animatorOverride)
        {
            if (!animatorOverride)
                return;

            if (!Controller || !Controller.Animator)
                return;
            
            // Conserve animator state
            var snapshot = CaptureAnimatorState(Controller.Animator);

            Controller.Animator.runtimeAnimatorController = animatorOverride;

            Controller.Animator.Rebind();
            Controller.Animator.Update(0f);

            RestoreAnimatorState(Controller.Animator, snapshot);
        }
        
        public void LockFacing(bool locked)
        {
            _facingLocked = locked;
        }
        
        #region Animator State Capture & Restore
        
        struct AnimatorSnapshot
        {
            public int stateHash;
            public float normalizedTime;
            public AnimatorControllerParameter[] parameters;
            public float[] values;
        }
        
        private AnimatorSnapshot CaptureAnimatorState(Animator animator)
        {
            var snapshot = new AnimatorSnapshot();

            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            snapshot.stateHash = stateInfo.fullPathHash;
            snapshot.normalizedTime = stateInfo.normalizedTime;

            snapshot.parameters = animator.parameters;
            snapshot.values = new float[snapshot.parameters.Length];

            for (int i = 0; i < snapshot.parameters.Length; i++)
            {
                var p = snapshot.parameters[i];
                snapshot.values[i] = p.type switch
                {
                    AnimatorControllerParameterType.Float => animator.GetFloat(p.nameHash),
                    AnimatorControllerParameterType.Int => animator.GetInteger(p.nameHash),
                    AnimatorControllerParameterType.Bool => animator.GetBool(p.nameHash) ? 1f : 0f,
                    _ => 0f
                };
            }

            return snapshot;
        }
        
        private void RestoreAnimatorState(Animator animator, AnimatorSnapshot snapshot)
        {
            animator.Play(snapshot.stateHash, 0, snapshot.normalizedTime);

            for (int i = 0; i < snapshot.parameters.Length; i++)
            {
                var p = snapshot.parameters[i];
                float v = snapshot.values[i];

                switch (p.type)
                {
                    case AnimatorControllerParameterType.Float:
                        animator.SetFloat(p.nameHash, v);
                        break;
                    case AnimatorControllerParameterType.Int:
                        animator.SetInteger(p.nameHash, Mathf.RoundToInt(v));
                        break;
                    case AnimatorControllerParameterType.Bool:
                        animator.SetBool(p.nameHash, v > 0.5f);
                        break;
                }
            }
        }
        
        #endregion
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position;
            Vector3 facingDir = Vector3.right * (int)CurrentFacing;

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origin, origin + facingDir);

            Gizmos.DrawSphere(origin + facingDir, 0.05f);
        }
#endif
    }
}
