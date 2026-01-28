using System;
using Core.Enum;
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
        
        protected override void OnInitialize()
        {
            // Explicit initial state — no ambiguity
            SetFacing(FacingDirection.Left, force: true);
        }
        
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

            OnFacingChanged?.Invoke(facing);
        }
        
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
