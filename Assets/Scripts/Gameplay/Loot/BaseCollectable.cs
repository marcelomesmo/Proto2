using Gameplay.Collectables;
using UnityEngine;

namespace Gameplay.Loot
{
    /*
        This class should:
        - Detect player collision
        - Notify inventory / resource system
        - Destroy or return to pool
    */
    [RequireComponent(typeof(Collider2D))]
    public class BaseCollectable : MonoBehaviour, ICollectable
    {
        [Header("Resource Data")]
        public ResourceSO resourceData;
   
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private LootMagnetReceiver magnet;
        [SerializeField] private Collider2D pickupCollider;
        public LootBounceController Bouncer;
    
        private bool _collected;
        
        [Header("Broadcast")]
        [SerializeField] protected IntEventChannelSO OnResourceCollected;

        protected virtual void Awake()
        {
            // Only auto-assign if not manually set
            if (!animator)
                animator = GetComponentInChildren<Animator>();
            if (!magnet)
                magnet = GetComponent<LootMagnetReceiver>();
            if (!pickupCollider)
                pickupCollider = GetComponentInChildren<Collider2D>();
            if (!Bouncer)
                Bouncer = GetComponent<LootBounceController>();
            
            pickupCollider.isTrigger = true;
        }
    
        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected)
                return;
            
            if (!other.CompareTag("Player"))
                return;

            Collect(other.gameObject);
        }

        public void Collect(GameObject collector)
        {
            _collected = true;
            
            pickupCollider.enabled = false;
            magnet.DisablePhysics();
            Bouncer.enabled = false;
            
            animator.SetTrigger("Interact");
        
            OnResourceCollected.RaiseEvent(resourceData.contribution);
        }

        public void FinishCollect()
        {
            Destroy(gameObject); // or return to pool
        }
    }
}
