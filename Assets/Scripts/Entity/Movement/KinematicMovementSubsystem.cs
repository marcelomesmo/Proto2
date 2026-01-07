using Entity.Stats;
using Enum;
using UnityEngine;

namespace Entity.Movement
{
    // ------------
    //  DEPRECATED
    // ------------
    /*
        This is a custom Kinematic Player controlled movement.
     */
    public class KinematicMovementSubsystem : EntityMovement
    {
        private PlayerStats _stats;
        
        // The current velocity of the entity.
        private Vector2 _velocity;
        private Vector2 _targetVelocity;
        private Vector2 _direction;

        private ContactFilter2D _contactFilter;
        private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
        private Vector2 _groundNormal;

        private const float MinMoveDistance = 0.001f;
        private const float ShellRadius = 0.01f;
        // The minimum normal (dot product) considered suitable for the entity sit on.
        private const float MinGroundNormalY = .65f;
    
        private bool _jump;
        private bool _stopJump;
        // A custom gravity coefficient applied to this entity.
        private const float GravityModifier = 1f;
        private JumpState _jumpState = JumpState.Grounded;
        
        [Header("Kinematic Movement Settings")]
        // A global jump modifier applied to all initial jump velocities.
        public float jumpModifier = 1.5f;
        // A global jump modifier applied to slow down an active jump when 
        // the user releases the jump input.
        public float jumpDeceleration = 0.5f;
        // Initial jump velocity at the start of a jump.
        public float jumpTakeOffSpeed = 7;
    
        protected virtual void Start()
        {
            _contactFilter.useTriggers = false;
            _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
            _contactFilter.useLayerMask = true;
        }
    
        protected override void OnInitialize()
        {
            Rb.bodyType = RigidbodyType2D.Kinematic;
        
            base.OnInitialize();
        
            if (Controller.Stats is PlayerStats playerStats)
                _stats = playerStats;
            else
                Debug.LogError("[KinematicMovementSubsystem] requires PlayerStats!");
        }

        protected override void OnDeinitialize()
        {
            Rb.bodyType = RigidbodyType2D.Dynamic;  // Why?
        
            base.OnDeinitialize();
        }
    
        public override void MoveTo(Vector2 target)
        {
            _targetVelocity = Vector2.zero;
            
            _direction = new Vector2(target.x, 0f);
        
            if (_jump && IsGrounded)
            {
                _velocity.y = jumpTakeOffSpeed * jumpModifier;
                _jump = false;
            }
            else if (_stopJump)
            {
                _stopJump = false;
                if (_velocity.y > 0)
                {
                    _velocity.y = _velocity.y * jumpDeceleration;
                }
            }

            CheckDirectionChange(_direction);
            
            /*
             Deprecated, moved to parent.
             if (_direction.x > 0.01f && !FacingRight)
            {
                FacingRight = true;
                OnDirectionChange?.Invoke();
            }
            else if (_direction.x < -0.01f && FacingRight)
            {
                FacingRight = false;
                OnDirectionChange?.Invoke();
            }*/

            // Animation triggers
            if (!Controller.Animator)
                return;
            
            Controller.Animator.SetBool("grounded", IsGrounded);
            Controller.Animator.SetFloat("velocityX", Mathf.Abs(_velocity.x) / _stats.moveSpeed);

            _targetVelocity = _direction * (_stats.moveSpeed * SpeedMultiplier);
        }
    
        protected virtual void FixedUpdate()
        {
            //if already falling, fall faster than the jump speed, otherwise use normal gravity.
            if (_velocity.y < 0)
                _velocity += GravityModifier * Physics2D.gravity * Time.deltaTime;
            else
                _velocity += Physics2D.gravity * Time.deltaTime;

            _velocity.x = _targetVelocity.x;

            IsGrounded = false;

            var deltaPosition = _velocity * Time.deltaTime;

            var moveAlongGround = new Vector2(_groundNormal.y, -_groundNormal.x);

            var move = moveAlongGround * deltaPosition.x;

            PerformMovement(move, false);

            move = Vector2.up * deltaPosition.y;

            PerformMovement(move, true);

        }

        private void PerformMovement(Vector2 move, bool yMovement)
        {
            var distance = move.magnitude;

            if (distance > MinMoveDistance)
            {
                //check if we hit anything in current direction of travel
                var count = Rb.Cast(move, _contactFilter, _hitBuffer, distance + ShellRadius);
                for (var i = 0; i < count; i++)
                {
                    var currentNormal = _hitBuffer[i].normal;

                    //is this surface flat enough to land on?
                    if (currentNormal.y > MinGroundNormalY)
                    {
                        IsGrounded = true;
                        // if moving up, change the groundNormal to new surface normal.
                        if (yMovement)
                        {
                            _groundNormal = currentNormal;
                            currentNormal.x = 0;
                        }
                    }
                    if (IsGrounded)
                    {
                        //how much of our velocity aligns with surface normal?
                        var projection = Vector2.Dot(_velocity, currentNormal);
                        if (projection < 0)
                        {
                            //slower velocity if moving against the normal (up a hill).
                            _velocity = _velocity - projection * currentNormal;
                        }
                    }
                    else
                    {
                        //We are airborne, but hit something, so cancel vertical up and horizontal velocity.
                        _velocity.x *= 0;
                        _velocity.y = Mathf.Min(_velocity.y, 0);
                    }
                    //remove shellDistance from actual move distance.
                    var modifiedDistance = _hitBuffer[i].distance - ShellRadius;
                    distance = modifiedDistance < distance ? modifiedDistance : distance;
                }
            }
            Rb.position = Rb.position + move.normalized * distance;
        }

        public override void Jump()
        {
            _jumpState = JumpState.PrepareToJump;
        }

        public override void StopJump()
        {
            _stopJump = true;
        }

        public override void UpdateJumpState()
        {
            _jump = false;
            switch (_jumpState)
            {
                case JumpState.PrepareToJump:
                    _jumpState = JumpState.Jumping;
                    _jump = true;
                    _stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        // TODO: Trigger PlayerJumped action events here case needed (e.g. play audio on jump should be through this action).
                        _jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        // TODO: Trigger PlayerFalling/InFlight -> Landed action events here case needed (e.g. play audio on fall and vfx should be through this action).
                        _jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    _jumpState = JumpState.Grounded;
                    // TODO: Trigger post-PlayerLanded action events here case needed (e.g. play audio on land and vfx should be through this action).
                    //vfxDust.Play();
                    break;
            }
        }

        public override void Stop()
        {
            base.Stop();
            
            //Debug.Log("[KinematicMovementSubsystem] stopped.");
        }
    }
}
