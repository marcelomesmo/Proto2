using System;
using System.Linq;
using Audio.Data;
using Entity.Stats;
using EventChannels;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entity.Movement
{
    /*
        This is a custom Player controlled movement that implements Dash and drop from platforms.
     */
    public class CustomPlayerMovement : EntityMovement
    {
        private PlayerStats _stats;
        
        // Movement
        [Header("Movement")]
        private float _targetVelocity;
        private Vector2 _direction;
        [SerializeField] private LayerMask groundMask;
        public float groundCheckDistance = 0.1f;

        // Sprint
        public bool CanSprint => _currentSprint > 0f;
        private float _currentSprint;
        private float _sprintTimer;
        private bool _isSprinting;
        
        // One Way Platform
        [Header("Drop Through")]
        [SerializeField] private float dropThroughDuration = 0.2f;
        private float _dropThroughTimer;
        private bool _droppingThrough;
        private bool _isOnOneWayPlatform;
        private bool _wasGrounded;
        
        // Collision Layers
        private int _playerLayer;
        private int _oneWayLayer;
        
        [Header("Broadcast")]
        [SerializeField] protected FloatEventChannelSO OnSprintChanged;
        [SerializeField] protected FeedbackEventChannelSO OnSprintStarted;
        [SerializeField] protected FeedbackEventChannelSO OnSprintEnded;
        [SerializeField] protected FeedbackEventChannelSO OnFallStarted;
        [SerializeField] protected FeedbackEventChannelSO OnFallEnded;
        [SerializeField] protected FeedbackEventChannelSO OnDirectionChanged;
        
        [Header("Audio")]
        [SerializeField] private AudioEvent landAudio;
        [SerializeField] private AudioEvent footstepAudio;
        //[SerializeField] private AudioEvent sprintAudio;
        
        public bool CanDropFromPlatform =>
            IsGrounded && _isOnOneWayPlatform && !_droppingThrough;
        public bool IsDropping => _droppingThrough;
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
        
            if (Controller.Stats is PlayerStats playerStats)
                _stats = playerStats;
            else
                Debug.LogError("[KinematicMovementSubsystem] requires PlayerStats!");
            
            _currentSprint = _stats.sprintStaminaTime;
            
            _playerLayer = LayerMask.NameToLayer("Player");
            _oneWayLayer = LayerMask.NameToLayer("OneWayPlatform");

            OnDirectionChange += DirectionChanged;  // Re-using the parent Action to trigger the EventChannel.
        }

        protected override void OnDeinitialize()
        {
            base.OnDeinitialize();

            _droppingThrough = false;

            Physics2D.IgnoreLayerCollision(
                _playerLayer,
                _oneWayLayer,
                false
            );
            
            OnDirectionChange -= DirectionChanged;
        }
        
        protected override void OnUpdate()
        {
            UpdateSprint(Time.deltaTime);
            
            UpdateDropThrough();
        }

        private void UpdateSprint(float dt)
        {
            if (_isSprinting)
            {
                _currentSprint -= dt;

                if (_currentSprint <= 0f)   // Reached end of sprint gauge (no sprint stamina).
                {
                    _currentSprint = 0f;
                    _isSprinting = false;
                    OnSprintEnded?.OnEventRaised(new FeedbackRequest());
                }
                
                OnSprintChanged?.RaiseEvent(_currentSprint/_stats.sprintStaminaTime);
            }
            else if(_currentSprint < _stats.sprintStaminaTime)
            {
                // Regen
                _currentSprint += dt * _stats.sprintRegenRate;
                _currentSprint = Mathf.Min(_currentSprint, _stats.sprintStaminaTime);
                
                OnSprintChanged?.RaiseEvent(_currentSprint/_stats.sprintStaminaTime);
            }
        }

        private void UpdateDropThrough()
        {
            if (!_droppingThrough)
                return;

            _dropThroughTimer -= Time.deltaTime;
            
            if (_dropThroughTimer <= 0f)
            {
                //if (_droppingThrough && IsGrounded) // Resets only when hitting the ground. We can add a timer here later to stop a bit.
                //    _droppingThrough = false;

                Physics2D.IgnoreLayerCollision(
                    _playerLayer,
                    _oneWayLayer,
                    false
                );
            }
        }

        protected override void OnFixedUpdate()
        {
            CheckGroundRaycast();

            UpdateAnimator();
        }

        private void CheckGroundRaycast()
        {
            bool wasGroundedLastFrame = IsGrounded;
            
            // BoxCast the ground. BoxCast is safer to avoid missing edges and slopes.
            Vector2 origin = new Vector2(   // Origin of the boxcast at the feet of the entity
                Bounds.center.x,
                Bounds.min.y
            );

            Vector2 size = new Vector2(     // Size of the BoxCast
                Bounds.size.x * 0.9f,
                0.05f
            );

            RaycastHit2D hit = Physics2D.BoxCast(
                origin,
                size,
                0f,
                Vector2.down,
                groundCheckDistance,
                groundMask
            );

            IsGrounded = hit.collider != null;
            
            _isOnOneWayPlatform =
                IsGrounded &&
                hit.collider.gameObject.layer == _oneWayLayer;
            
            if (!wasGroundedLastFrame && IsGrounded)
            {
                HandleLanded();
            }
            
            // Debug visualization (editor only)
            /*Debug.DrawRay(
                origin,
                Vector2.down * groundCheckDistance,
                IsGrounded ? Color.green : Color.red
            );
#if UNITY_EDITOR
            Debug.DrawLine(
                origin,
                origin + Vector2.down * (groundCheckDistance * 5),
                _isOnOneWayPlatform ? Color.cyan : (IsGrounded ? Color.green : Color.red)
            );
#endif*/
        }
        
        private void UpdateAnimator()
        {
            // Animation triggers
            if (!Controller.Animator)
                return;
            
            Controller.Animator.SetBool("grounded", IsGrounded);
            float normalizedSpeed = Mathf.Abs(Rb.linearVelocity.x) / _stats.moveSpeed;
            Controller.Animator.SetFloat("velocityX", normalizedSpeed);
        }
        
        private void HandleLanded()
        {
            if (_droppingThrough)
            {
                _droppingThrough = false;
                OnFallEnded?.OnEventRaised(new FeedbackRequest());
                
                if(landAudio)
                    EntityAudio.Play(landAudio);
            }
        }
        
        public override void MoveTo(Vector2 target)
        {
            if (!CanRbMove)
                return;

            _direction = target;
            _targetVelocity = _stats.moveSpeed * SpeedMultiplier;

            if (_isSprinting)
                _targetVelocity *= _stats.sprintSpeedMultiplier;

            Rb.linearVelocity = new Vector2(
                _direction.x * _targetVelocity,
                Rb.linearVelocity.y
            );

            CheckDirectionChange(_direction);
        }

        public void SetSprintActive(bool active)
        {
            if (active && CanSprint)
            {
                _isSprinting = true;
                OnSprintStarted?.OnEventRaised(new FeedbackRequest());
                //EntityAudio.Play(sprintAudio);
            }
            else
            {
                _isSprinting = false;
                OnSprintEnded?.OnEventRaised(new FeedbackRequest());
                // TODO: Check if sprint ended because player released key or because depleted. In case depleted, treat separately (play audio case necessary, etc).
            }
        }
        
        public void DropFromPlatform()
        {
            if (!CanDropFromPlatform)
                return;
            
            Stop();

            _droppingThrough = true;
            _dropThroughTimer = dropThroughDuration;

            Physics2D.IgnoreLayerCollision(
                _playerLayer,
                _oneWayLayer,
                true
            );
            
            OnFallStarted?.OnEventRaised(new FeedbackRequest());
            //EntityAudio.Play(onewayplatdropAudio);
        }
        
        private void DirectionChanged()
        {
            FeedbackRequest request = new FeedbackRequest
            {
                Intensity = FacingRight ? 1 : -1        // Passing direction through intensity channel
            };
            OnDirectionChanged?.OnEventRaised(request);
            //EntityAudio.Play(directionChangeDriftAudioLOL);
        }

        public override void OnFootstep()
        {
            if(footstepAudio)
                EntityAudio.Play(footstepAudio);
        }
    }
}
