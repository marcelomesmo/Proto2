using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Core.Gameplay.Entity.AnimationRelay
{
    public class EntityAnimationEventRelay : MonoBehaviour
    {
        private EntityHealth _healthSubsystem;
        private EntityAttackSubsystem _attackSubsystem;
        private EntityMovement _movementSubsystem;
        private EntityPresentationSubsystem _presentationSubsystem;
        
        private void Awake()
        {
            _healthSubsystem = GetComponentInParent<EntityHealth>();
            _attackSubsystem = GetComponentInParent<EntityAttackSubsystem>();
            _movementSubsystem = GetComponentInParent<EntityMovement>();
            _presentationSubsystem = GetComponentInParent<EntityPresentationSubsystem>();
        }

        public void SetInvulnerableTrue()
        {
            if (_healthSubsystem) _healthSubsystem.SetInvulnerable(true);
        }

        public void SetInvulnerableFalse()
        {
            if (_healthSubsystem) _healthSubsystem.SetInvulnerable(false);
        }

        public void OnDeathAnimationFinished()
        {
            _presentationSubsystem?.NotifyDeathAnimationFinished();
        }
        
        public void OnSpawnAnimationFinished()
        {
            _presentationSubsystem?.NotifySpawnAnimationFinished();
        }
        
        public void OnAttackFrameFinished()
        {
            _attackSubsystem?.AttackResolveFrame();
        }

        public void PlayFootstep()
        {
            _movementSubsystem.OnFootstep();
        }
    }
}