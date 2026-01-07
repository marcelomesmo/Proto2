using Enum;
using Gameplay.Projectile;
using Gameplay.VFX;
using Services.Manager;
using UnityEngine;

namespace Entity.Effects
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

        [Header("VFX Prefabs")]
        [SerializeField] private GameObject damagePopupPrefab;
        //[SerializeField] private GameObject hitVfxPrefab;
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
            SpawnDamagePopup(prefab, damagePopupAnchor, payload.hitData.damage);
            
            // TODO: Is this the same for both Enemy and Player?
            
            // Hit VFX treatment
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
            
            // Trigger shader flash
            if (hitFlash)
                hitFlash.Flash();
        }

        private void OnDied()
        {
            Spawn(deathVfxPrefab, deathAnchor);
        }

        public void PlayDust()
        {
            Spawn(dustVfxPrefab, movementAnchor);
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
