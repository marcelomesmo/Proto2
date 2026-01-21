using System;
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
    public sealed class BaseLoot : MonoBehaviour, ICollectable
    {
        [Header("Data")]
        public LootSO lootData;
   
        [Header("References")]
        [SerializeField] private Animator animator;
        
        private LootController _lootController;
        private Collider2D _collider;
        private bool _collected;
        
        [Header("Broadcast")]
        [SerializeField] private IntEventChannelSO OnLootCollected;

        private void Awake()
        {
            _lootController = GetComponent<LootController>();
            _collider = GetComponent<Collider2D>();

            _collider.isTrigger = true;
            
            // Only auto-assign if not manually set
            if (!animator)
                animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            _collected = false;
            _collider.enabled = true;

            _lootController.Initialize(lootData);
            
            _lootController.MagnetArrived += HandleMagnetArrived;
        }

        private void OnDisable()
        {
            _lootController.MagnetArrived -= HandleMagnetArrived;
        }

        private void HandleMagnetArrived() => Collect();

        public void Collect()
        {
            if (_collected)
                return;
            
            _collected = true;
            _collider.enabled = false;
            
            animator?.SetTrigger("Interact");
        
            OnLootCollected?.RaiseEvent(lootData.contribution);
        }

        public void FinishCollect()
        {
            Destroy(gameObject); // or return to pool
        }
    }
}
