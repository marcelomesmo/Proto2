using Core.EventChannels;
using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Loot
{
    /*
        This class should:
        - Detect player collision
        - Notify inventory / resource system
        - Destroy or return to pool
    */
    [RequireComponent(typeof(Collider2D))]
    public sealed class BaseLoot : MonoBehaviour, ICollectable
    {
        [Header("Resource Data")]
        public LootSO lootData;
   
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private LootMagnetReceiver magnet;
        [SerializeField] private Collider2D pickupCollider;
        public LootBounceController Bouncer;
    
        private bool _collected;
        
        [Header("Broadcast")]
        [SerializeField]
        private IntEventChannelSO OnResourceCollected;

        private void Awake()
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

        private void OnTriggerEnter2D(Collider2D other)
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
        
            OnResourceCollected.RaiseEvent(lootData.contribution);
        }

        public void FinishCollect()
        {
            Destroy(gameObject); // or return to pool
        }
    }
}
