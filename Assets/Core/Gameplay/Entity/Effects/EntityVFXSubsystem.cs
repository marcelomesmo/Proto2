using System;
using System.Collections.Generic;
using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity.Subsystem;
using Core.Gameplay.Entity.Tags;
using Core.Services.Manager;
using Core.VFX;
using UnityEngine;

namespace Core.Gameplay.Entity.Effects
{
    public class EntityVFXSubsystem : BaseSubsystem
    {
        /*
         * This subsystem:

            Subscribes to entity events
            Owns VFX anchors
            Maps semantic events → prefabs
            Calls VFXPoolManager
         */
        
        private EntityHealth _healthSubsystem;
        
        [Header("Anchors")]
        [SerializeField] private Transform damagePopupAnchor;
        [SerializeField] private Transform hitAnchor;
        [SerializeField] private Transform deathAnchor;
        [SerializeField] private Transform movementAnchor;
        [SerializeField] private Transform statusEffectHeadAnchor;
        [SerializeField] private Transform statusEffectDamageAnchor;

        [Header("VFX Prefabs")]
        [SerializeField] private GameObject damagePopupPrefab;
        [SerializeField] private GameObject deathVfxPrefab;
        [SerializeField] private GameObject dustVfxPrefab;
        
        [Header("References")]
        public HitFlash hitFlash;
        
        protected override void OnInitialize()
        {
            _healthSubsystem = Controller.GetComponent<EntityHealth>();
            _healthSubsystem.DamageTaken += OnDamageTaken;
            //health.Died += OnDied;
        }
        
        protected override void OnDeinitialize()
        {
            // Unsubscribe unconditionally and null the delegate owner -> avoid GC or pool reuse problems.
            // "Whoever subscribes is 100% responsible for unsubscribing — regardless of order."
            _healthSubsystem.DamageTaken -= OnDamageTaken;
            _healthSubsystem = null;
        }

        private void OnDamageTaken(DamagePayload payload /*, bool isCritical*/)    // TODO: pass critical in payload.
        {
            //var prefab = isCritical
            //    ? damagePopupCriticalPrefab
            //    : damagePopupPrefab;

            //Spawn(damagePopupPrefab, hitAnchor);
            var prefab = damagePopupPrefab;
            SpawnDamagePopup(prefab, damagePopupAnchor, payload.ResolvedDamage);
            
            // TODO: Is this the same for both Enemy and Player?
            
            // Hit VFX treatment
            if (payload.hitData != null)
            {
                Spawn(payload.hitData.hitVFX, hitAnchor);

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
            
            // Trigger shader flash
            if (hitFlash)
                hitFlash.Flash();
        }
        
        #region Tag VFX 
        
        private readonly Dictionary<GameplayTag, GameObject> _activeTagVfx = new();
        
        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (!tag.vfxPrefab)
            {
                Debug.LogWarning(
                    $"[EntityVFXSubsystem] Tag '{tag.name}' has no VFX prefab.",
                    this);
                return;
            }
            
            if (_activeTagVfx.ContainsKey(tag))
                return; // already playing
            
            Transform anchor = ResolveTagAnchor(tag);
            if (!anchor)
                return;

            var vfx = Spawn(tag.vfxPrefab, anchor, true);
            if (!vfx)
                return;

            _activeTagVfx.Add(tag, vfx);
        }
        
        protected override void HandleTagRemoved(GameplayTag tag)
        {
            // despawn pooled instance, etc.
            if (!_activeTagVfx.TryGetValue(tag, out var vfx))
                return;

            if (vfx)
                VFXPoolManager.Instance.Release(tag.vfxPrefab, vfx);

            _activeTagVfx.Remove(tag);
        }
        
        private Transform ResolveTagAnchor(GameplayTag tag)
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

        // Stateless VFX spawn (do not require runtime data, are fully defined by the prefab): Hit sparks, dust, death explosion, spawn effect.
        private GameObject Spawn(GameObject prefab, Transform anchor, bool isAttached = false)
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
            
            if (vfx.TryGetComponent(out ParticleSystemVFX ps))
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
    }
}
