using System.Collections.Generic;
using Core.Enum;
using Core.EventChannels;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity.Tags;
using Core.Services.Manager;
using Core.VFX;
using UnityEngine;

namespace Core.Gameplay.Entity.Subsystem
{
    /*  This subsystem:

        Subscribes to entity events
        Owns VFX anchors
        Maps semantic events → prefabs
        Calls VFXPoolManager
     */
    public class EntityVFXSubsystem : BaseSubsystem
    {
        [Header("Anchors")]
        [SerializeField] private Transform damagePopupAnchor;
        [SerializeField] private Transform hitAnchor;
        [SerializeField] private Transform deathAnchor;
        [SerializeField] private Transform movementAnchor;
        [SerializeField] private Transform statusEffectHeadAnchor;      // Above head feedback
        [SerializeField] private Transform statusEffectDamageAnchor;    // Body feedback
        [SerializeField] private Transform attackCastAnchor;

        [Header("VFX Prefabs")]
        [SerializeField] private GameObject damagePopupPrefab;
        [SerializeField] private GameObject deathVfxPrefab;
        [SerializeField] private GameObject dustVfxPrefab;
        
        [Header("References")]
        public HitFlash hitFlash;
        
        [Header("Broadcast")]
        [SerializeField] private FeedbackEventChannelSO cameraShakeEvent;
        
        private EntityHealth _healthSubsystem;              // MANDATORY ?
        private EntityAttackSubsystem _attackSubsystem;
        
        protected override void OnInitialize()
        {
            _healthSubsystem = Controller.GetComponent<EntityHealth>();
            if (!_healthSubsystem)
            {
                Debug.LogError(
                    $"[EntityVFXSubsystem] Missing EntityHealth on {Controller.name}",
                    this);
                enabled = false;
                return;
            }
            
            _healthSubsystem.OnDamageTaken += HandleDamageTaken;
            //health.Died += OnDied;
            
            _attackSubsystem = Controller.GetComponent<EntityAttackSubsystem>();
            if(_attackSubsystem)
                _attackSubsystem.OnAttackExecuted += HandleAttackExecuted;
        }
        
        protected override void OnDeinitialize()
        {
            // Unsubscribe unconditionally and null the delegate owner -> avoid GC or pool reuse problems.
            // "Whoever subscribes is 100% responsible for unsubscribing — regardless of order."
            _healthSubsystem.OnDamageTaken -= HandleDamageTaken;
            _healthSubsystem = null;
            
            if(_attackSubsystem)
                _attackSubsystem.OnAttackExecuted -= HandleAttackExecuted;
            _attackSubsystem = null;
        }

        #region Damage and Attack
        
        private void HandleDamageTaken(DamagePayload payload)
        {
            // 1. Spawn Damage Popup VFX lettering.
            var prefab = damagePopupPrefab;
            SpawnDamagePopup(prefab, damagePopupAnchor, payload.ResolveDamage());
            //var prefab = isCritical
            //    ? damagePopupCriticalPrefab
            //    : damagePopupPrefab;
            
            // 2. Spawn Hit VFX treatment.
            if (payload.attack != null)
            {
                Spawn(payload.attack.Data.hitVFX, hitAnchor);

                switch (payload.attack.Data.hitTypes)
                {
                    case HitTypes.Physical:
                        // Spawn additional VFX if required
                        //Spawn(genericVFX_physical, hitAnchor);
                        break;
                    case HitTypes.Explosive:
                        // Spawn explosion VFX case wanted
                        //Spawn(genericVFX_explosive, hitAnchor);
                        break;
                    //etc
                    default:
                        break;
                }
            }
            
            // 3. Handle Flash shader.
            if (hitFlash)
                hitFlash.Flash();
        }
        
        private void HandleAttackExecuted(AttackInstance attack)
        {
            if(attack.Data == null)
                return;

            if(attack.Data.castVFX == null)
                return;
            
            Spawn(attack.Data.castVFX, attackCastAnchor);
        }
        
        #endregion
        
        #region Camera
        
        private void CameraShakeExample(FeedbackRequest obj)
        {
            // Camera Shake example
            cameraShakeEvent.OnEventRaised(new FeedbackRequest 
            {
                Scope = FeedbackScope.Global,
                Type = FeedbackType.Land,
                Intensity = 0.05f
            });
        }
        
        #endregion
        
        #region Tag VFX 
        
        private readonly Dictionary<GameplayTag, GameObject> _activeTagVfx = new();
        
        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == null)
                return;
            
            // This tag has no visual representation
            if (!tag.vfxPrefab)
            {
                Debug.LogWarning(
                    $"[EntityVFXSubsystem] Tag '{tag.name}' has no VFX prefab.",
                    this);
                return;
            }
            
            Transform anchor = ResolveAnchorForTag(tag);
            if (!anchor)
            {
                Debug.LogWarning(
                    $"[EntityVFXSubsystem] No anchor for tag '{tag.name}' on {Controller.name}",
                    this);
                return;
            }
            
            // Prevent double-spawn
            if (_activeTagVfx.ContainsKey(tag))
                return; // already playing

            var vfx = Spawn(tag.vfxPrefab, anchor, true);
            if (!vfx)
                return;

            _activeTagVfx.Add(tag, vfx);
        }
        
        protected override void HandleTagRemoved(GameplayTag tag)
        {
            if (tag == null)
                return;
            
            // despawn pooled instance, etc.
            if (!_activeTagVfx.TryGetValue(tag, out var vfx))
                return;

            if (vfx)
                VFXPoolManager.Instance.Release(tag.vfxPrefab, vfx);

            _activeTagVfx.Remove(tag);
        }
        
        private Transform ResolveAnchorForTag(GameplayTag tag)
        {
            if (tag == Controller.Stats.burnTag)
                return statusEffectDamageAnchor;

            if (tag == Controller.Stats.stunTag || tag == Controller.Stats.slowTag)
                return statusEffectHeadAnchor;

            return null;
        }

        #endregion
        
        #region Fixed VFX
        
        private void OnDied()
        {
            Spawn(deathVfxPrefab, deathAnchor);
        }

        public void PlayDust()
        {
            Spawn(dustVfxPrefab, movementAnchor);
        }
        
        #endregion

        #region Spawn VFX Handling
        
        // Stateless VFX spawn (do not require runtime data, are fully defined by the prefab): Hit sparks, dust, death explosion, spawn effect.
        protected GameObject Spawn(GameObject prefab, Transform anchor, bool isAttached = false)
        {
            if (!prefab)
                return null;
            
            if (!anchor)
            {
                Debug.LogWarning(
                    $"[EntityVFXSubsystem] Missing anchor for VFX '{prefab.name}' on {Controller.name}",
                    this);
                return null;
            }

            var vfx = VFXPoolManager.Instance.Spawn(prefab);
            if (!vfx)
            {
                Debug.LogWarning(
                    $"[EntityVFXSubsystem] Couldn't create VFX '{prefab.name}' on {Controller.name}",
                    this);
                return null;
            }
            
            vfx.transform.SetPositionAndRotation(anchor.position, Quaternion.identity);
            
            if (isAttached)
            {
                vfx.transform.SetParent(anchor, false);
                vfx.transform.localPosition = Vector3.zero;
                vfx.transform.localRotation = Quaternion.identity;
            }
            else
                vfx.transform.SetPositionAndRotation(anchor.position, Quaternion.identity);
            
            if (vfx.TryGetComponent(out PooledParticleSystemVFX ps))
                ps.Initialize(prefab);

            return vfx;
        }

        private void SpawnDamagePopup(
            GameObject prefab,
            Transform anchor,
            int amount)
        {
            if (prefab == null || anchor == null)
                return;

            var vfx = VFXPoolManager.Instance.Spawn(prefab);
            vfx.transform.SetPositionAndRotation(anchor.position, Quaternion.identity);

            vfx.GetComponent<FloatingDamageVFX>()
                .Initialize(amount, prefab);
        }
        
        #endregion
    }
}
