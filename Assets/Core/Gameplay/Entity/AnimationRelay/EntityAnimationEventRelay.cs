using Core.Gameplay.Entity.Subsystem;
using Entity;
using UnityEngine;

namespace Core.Gameplay.Entity.AnimationRelay
{
    public class EntityAnimationEventRelay : MonoBehaviour
    {
        private EntityHealth _healthSubsystem;
        private EntityAttackSubsystem _attackSubsystem;
        private EntityMovement _movementSubsystem;
        
        private void Awake()
        {
            _healthSubsystem = GetComponentInParent<EntityHealth>();
            _attackSubsystem = GetComponentInParent<EntityAttackSubsystem>();
            _movementSubsystem = GetComponentInParent<EntityMovement>();
        }

        public void OnDeathAnimationFinished()
        {
            _healthSubsystem?.NotifyDeathAnimationFinished();
        }

        public void SetInvulnerableTrue()
        {
            if (_healthSubsystem) _healthSubsystem.SetInvulnerable(true);
        }

        public void SetInvulnerableFalse()
        {
            if (_healthSubsystem) _healthSubsystem.SetInvulnerable(false);
        }
        
        public void OnSpawnAnimationFinished()
        {
            _healthSubsystem?.NotifySpawnAnimationFinished();
        }
        
        public void OnAttackFrameFinished()
        {
            _attackSubsystem?.OnAttackHit();
        }

        public void PlayFootstep()
        {
            _movementSubsystem.OnFootstep();
        }
    }
}