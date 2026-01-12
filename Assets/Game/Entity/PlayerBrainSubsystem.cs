using System;
using Core.Enum;
using Core.Gameplay.Entity.Movement;
using Core.Gameplay.Entity.Subsystem;
using Core.Gameplay.Entity.Tags;
using Core.Interfaces;
using Entity.Stats;
using Enum;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Entity
{
    [RequireComponent(typeof(EntityMovement))]
    public class PlayerBrainSubsystem : EntityBrainSubsystem
    {
        public ActionState actionState = ActionState.Idle;
        
        private InputAction _mMoveAction;
        private InputAction _mAttackAction;
        private InputAction _mSecondaryAttackAction;
        private InputAction _mInteractAction;
        private InputAction _mDropAction;
        private InputAction _mSprintAction;
        
        //private bool _finishedInitializing;
        
        // Interaction
        private IInteractable _currentInteractable;

        // Attack
        //public WeaponManager primaryWeapon;
        //public WeaponManager secondaryWeapon;
        public AimState aimState = AimState.Right;
        //private GameObject _reloadBar;
        
        // Local References
        private CustomPlayerMovement _movement;
        private PlayerStats _stats;
        
        private void Awake()
        {
            _mMoveAction = InputSystem.actions.FindAction("Player/Move");
            _mAttackAction = InputSystem.actions.FindAction("Player/Attack");
            _mSecondaryAttackAction = InputSystem.actions.FindAction("Player/SecondaryAttack");
            _mInteractAction = InputSystem.actions.FindAction("Player/Interact");
            _mDropAction = InputSystem.actions.FindAction("Player/OneWayDrop");
            _mSprintAction = InputSystem.actions.FindAction("Player/Sprint");
           
            _mMoveAction.Enable();
            _mAttackAction.Enable();
            _mSecondaryAttackAction.Enable();
            _mInteractAction.Enable();
            _mDropAction.Enable();
            _mSprintAction.Enable();
        }

        protected override void OnInitialize()
        {
            //_reloadBar = transform.Find("ReloadBar").gameObject;
            
            _movement = GetComponent<CustomPlayerMovement>();
            
            if (Controller.Stats is PlayerStats playerStats)
                _stats = playerStats;
            //else
            //    Debug.LogError("[PlayerBrainSubsystem] requires PlayerStats!");

            //controlEnabled = true;    Moved to HandleTagAdded
            //_finishedInitializing = true;
        }
        
        protected override void OnDeinitialize()
        {
            // just in case
            //controlEnabled = false;
            //_finishedInitializing = false;
        }

        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == _stats.spawnFinishedTag)
                controlEnabled = true;

            if (tag == _stats.deadTag)
            {
                controlEnabled = false;
                _movement.Stop();
            }
        }

        protected override void OnUpdate()
        {
            //if (!_finishedInitializing)
            //    return;

            if (!controlEnabled)    // todo: Move this to the specific CanInteract, CanReload, etc.
                return;

            if (Controller.IsDead)  // todo: Same
                return;

            TryMove(); // Update physics and Movement subsystem.
            
            ChangeAimDirection(aimState);

            TryReload();
            
            TryShoot();

            TryInteract();
        }

        private void TryInteract()
        {
            if (!CanProcessInput)
                return;
            
            // Interact with something aka buildings
            if(_mInteractAction.IsPressed() 
               && _currentInteractable != null
               && _movement.IsGrounded
               && actionState != ActionState.Shooting && actionState != ActionState.Reloading)
            {
                _movement.Stop();
                
                if(actionState != ActionState.Interacting)
                {
                    actionState = ActionState.Interacting;
                    
                    _currentInteractable.Interact();
                }
            }
            else if(_mInteractAction.WasReleasedThisFrame()
                    && _currentInteractable != null
                    && actionState == ActionState.Interacting)
            {
                actionState = ActionState.Idle;
                _currentInteractable.StopInteract();
            }
        }

        private void TryShoot()
        {
            if(StoppedShooting())
            {
                actionState = ActionState.Idle;
                _movement.SetSpeedMultiplier(GameConst.ENTITY_BASE_SPEED_MULTIPLIER);
            }

            if (!CanShoot) 
                return;
            /*
            // Primary Weapon
            if (PrimaryPressed())
            {
                actionState = ActionState.Shooting;
                _movement.SetSpeedMultiplier(_stats.shootingSpeedMultiplier);
                //primaryWeapon.Shoot();
            }
            // Secondary Weapon
            else if (SecondaryPressed())
            {
                actionState = ActionState.Shooting;
                _movement.SetSpeedMultiplier(_stats.shootingSpeedMultiplier);
                //secondaryWeapon.Shoot();
            }
            */
        }

        private void TryReload()
        {
            /*
             if(primaryWeapon.IsAmmoEmpty)
            {
                if(actionState != ActionState.Reloading)
                {
                    actionState = ActionState.Reloading;
                    
                    _movement.SetSpeedMultiplier(GameConst.ENTITY_BASE_SPEED_MULTIPLIER);
                    
                    _reloadBar.SetActive(true);
                    primaryWeapon.StartReload();
                }
                _reloadBar.GetComponent<ProgressBar>().SetProgress(primaryWeapon.GetReloadProgress());
                primaryWeapon.Reload();
            }
            else if (actionState == ActionState.Reloading)
            {
                actionState = ActionState.Idle;
                _reloadBar.SetActive(false);    // Could replace this with a listener to primaryWeapon.m_WeaponReloaded.
            }
            
            // RELOAD - Secondary Weapon
            if (!secondaryWeapon.IsAmmoFull)
            {
                // Autoreload
                secondaryWeapon.ConstantReload();
            }
            */
        }

        private void TryMove()
        {
            if(!CanMove)
                return;
            
            if (_movement.IsGrounded && _mSprintAction.WasPressedThisFrame())
                _movement.SetSprintActive(true);
            else if(_mSprintAction.WasReleasedThisFrame())
                _movement.SetSprintActive(false);
            
            Vector2 move = _mMoveAction.ReadValue<Vector2>();
            _movement.MoveTo(move);
            
            if (_movement.CanDropFromPlatform && _mDropAction.WasPressedThisFrame())
                _movement.DropFromPlatform();
        }

        private void ChangeAimDirection(AimState newState)
        {
            //if(!primaryWeapon || !secondaryWeapon)
            //    throw new NullReferenceException("Primary or Secondary weapon reference is missing.");

            // Check aim direction based on input
            // Check Left / Right direction
            if (_mMoveAction.ReadValue<Vector2>().x > 0.01f)
                aimState = AimState.Right;
            else if (_mMoveAction.ReadValue<Vector2>().x < -0.01f)
                aimState = AimState.Left;

            // Check Up direction or reset based on input
            if(_mMoveAction.ReadValue<Vector2>().y > GameConst.Y_INPUT_THRESHOLD)
            {
                if(aimState == AimState.Right)
                    aimState = AimState.TopRight;
                else if(aimState == AimState.Left)
                    aimState = AimState.TopLeft;
            }
            else //if (_mMoveAction.ReadValue<Vector2>().y < -GameConst.Y_INPUT_THRESHOLD)
            {
                if(aimState == AimState.TopRight)
                    aimState = AimState.Right;
                else if(aimState == AimState.TopLeft)
                    aimState = AimState.Left;
            }
            
            // Aim weapon towards that direction
            //primaryWeapon.ChangeWeaponDirection(newState);
            //secondaryWeapon.ChangeWeaponDirection(newState);
        }
        
        #region Helpers

        private bool CanShoot =>
            CanProcessInput &&
            actionState is not (ActionState.Reloading or ActionState.Turning or ActionState.Interacting);
        private bool CanMove =>
            CanProcessInput &&
            actionState is not (ActionState.Interacting);
        //private bool PrimaryPressed() =>
        //    _mAttackAction.IsPressed() && !primaryWeapon.IsAmmoEmpty && !_mSecondaryAttackAction.IsPressed();
        //private bool SecondaryPressed() =>
        //    _mSecondaryAttackAction.IsPressed() && !secondaryWeapon.IsAmmoEmpty && !_mAttackAction.IsPressed();
        private bool StoppedShooting() =>
            actionState == ActionState.Shooting &&
            (_mAttackAction.WasReleasedThisFrame() || _mSecondaryAttackAction.WasReleasedThisFrame());
        private bool CanProcessInput =>
            controlEnabled &&
            !Controller.IsDead &&
            !_movement.IsDropping &&
            _movement.IsGrounded;

        #endregion Helpers
        
        #region Interactables
        private void OnTriggerEnter2D(Collider2D other)
        {
            IInteractable interactable = other.GetComponent<IInteractable>();
            if (interactable != null)
                _currentInteractable = interactable;
            
            var collectible = other.GetComponent<ICollectable>();
            if (collectible != null)
                collectible.Collect(this.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<IInteractable>() == _currentInteractable)
            {
                _currentInteractable = null;
            }
        }

        public IInteractable GetCurrentInteractable()
        {
            return _currentInteractable;
        }
        #endregion
    }
}
