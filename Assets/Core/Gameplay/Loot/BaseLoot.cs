using Core.EventChannels;
using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Loot
{
    /*
        data + reward + pooling + animation
    */
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(LootController))]
    public class BaseLoot : MonoBehaviour, ICollectable
    {
        [Header("Data")]
        public LootSO lootData;
   
        [Header("References")]
        [SerializeField] protected Animator Animator;
        
        protected LootController LootController;
        protected Collider2D Collider;
        protected bool Collected;

        protected virtual void Awake()
        {
            LootController = GetComponent<LootController>();
            Collider = GetComponent<Collider2D>();

            Collider.isTrigger = true;
            
            // Only auto-assign if not manually set
            if (!Animator)
                Animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            Collected = false;
            Collider.enabled = true;

            LootController.Initialize(lootData);
            
            LootController.MagnetArrived += HandleMagnetArrived;
        }

        private void OnDisable()
        {
            LootController.MagnetArrived -= HandleMagnetArrived;
        }

        private void HandleMagnetArrived() => Collect();

        public virtual void Collect()
        {
            if (Collected)
                return;
            
            Collected = true;
            Collider.enabled = false;
            
            Animator?.SetTrigger("Interact");
        }

        public void FinishCollect()
        {
            Destroy(gameObject); // or return to pool
        }
    }
}
