using Core.Enum;
using Core.EventChannels;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity.Effects;
using Core.Gameplay.Entity.Subsystem;
using Core.Services.Manager;
using Core.VFX;
using UnityEngine;

namespace Entity.Effects
{
    public class PlayerVFXSubsystem : BaseSubsystem
    {
        [Header("VFX")]
        [SerializeField] private AfterimageEmitter sprintAfterimageEmitter;
        [SerializeField] private AfterimageEmitter fallAfterimageEmitter;
        [SerializeField] private ParticleSystem dustVFX;
        
        [Header("Broadcast")]
        [SerializeField] private FeedbackEventChannelSO sprintStartedEvent;
        [SerializeField] private FeedbackEventChannelSO sprintEndedEvent;
        [SerializeField] private FeedbackEventChannelSO fallStartedEvent;
        [SerializeField] private FeedbackEventChannelSO fallEndedEvent;
        [SerializeField] private FeedbackEventChannelSO directionChangedEvent;
        [SerializeField] private FeedbackEventChannelSO cameraShakeEvent;
        
        private EntityHealth _healthSubsystem;
        
        // TODO: Can we move these 2 to initialize/deinitialize?
        private void OnEnable()
        {
            sprintStartedEvent.OnEventRaised += HandleSprintStarted;
            sprintEndedEvent.OnEventRaised += HandleSprintEnded;
            
            fallStartedEvent.OnEventRaised += HandleFallStarted;
            fallEndedEvent.OnEventRaised += HandleFallEnded;

            directionChangedEvent.OnEventRaised += HandleDirectionChanged;
        }

        private void OnDisable()
        {
            sprintStartedEvent.OnEventRaised -= HandleSprintStarted;
            sprintEndedEvent.OnEventRaised -= HandleSprintEnded;
            
            fallStartedEvent.OnEventRaised -= HandleFallStarted;
            fallEndedEvent.OnEventRaised -= HandleFallEnded;
            
            directionChangedEvent.OnEventRaised -= HandleDirectionChanged;
        }

        protected override void OnInitialize()
        {
            _healthSubsystem = Controller.GetComponent<EntityHealth>();
            _healthSubsystem.DamageTaken += OnDamageTaken;
        }

        protected override void OnDeinitialize()
        {
            _healthSubsystem.DamageTaken -= OnDamageTaken;
            _healthSubsystem = null;
        }

        //
        //  SPRINT
        //
        private void HandleSprintStarted(FeedbackRequest obj)
        {
            sprintAfterimageEmitter.SetActive(true);
        }
        private void HandleSprintEnded(FeedbackRequest obj)
        {
            sprintAfterimageEmitter.SetActive(false);
        }

        //
        //  FALL
        //
        private void HandleFallStarted(FeedbackRequest obj)
        {
            fallAfterimageEmitter.SetActive(true);
        }
        private void HandleFallEnded(FeedbackRequest obj)       // On Land Event
        {
            fallAfterimageEmitter.SetActive(false);
            // Ground dust
            dustVFX.Play(true);
            // Camera Shake
            cameraShakeEvent.OnEventRaised(new FeedbackRequest 
            {
                Scope = FeedbackScope.Global,
                Type = FeedbackType.Land,
                Intensity = 0.05f
            });
        }
        
        //
        //  DUST VFX
        //
        private void HandleDirectionChanged(FeedbackRequest obj)
        {
            dustVFX.Play(true);
        }
        
        // 
        //  DAMAGE
        //
        // TODO: Move all of this to a base class EntityVFXSubsystem. Refactor current EntityVFXSubsystem to be EnemyVFXSubsystem and converge commons to base class.
        private void OnDamageTaken(DamagePayload payload)
        {
            // Hit VFX treatment
            Spawn(payload.hitData.hitVFX, this.transform);

            switch (payload.hitData.hitType)
            {
                case HitType.Physical:
                    // Spawn additional VFX if required
                    //Spawn(genericVFX_physical, hitAnchor);
                    break;
                case HitType.Explosive:
                    // Spawn explosion VFX case wanted
                    //Spawn(genericVFX_explosive, hitAnchor);
                    break;
                //etc
                default:
                    break;
            }
        }
        
        // Stateless VFX spawn (do not require runtime data, are fully defined by the prefab): Hit sparks, dust, death explosion, spawn effect.
        private void Spawn(GameObject prefab, Transform anchor)
        {
            if (prefab == null || anchor == null)
                return;

            var vfx = VFXPoolManager.Instance.Spawn(prefab);
            vfx.transform.SetPositionAndRotation(anchor.position, Quaternion.identity);
            
            vfx.GetComponent<ParticleSystemVFX>()
                .Initialize(prefab);
        }
    }
}