using Core.Enum;
using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Loot
{
     /*
        Owns:
        - loot data reference
        - collection eligibility
        - reward application hook
        - collect animation
        - final destroy/pool point

        Does not own:
        - launch physics
        - bounce physics
        - magnet movement
    */
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(LootController))]
    public class BaseLoot : MonoBehaviour, ICollectable
    {
        private LootSO _lootData;
        protected LootSO LootData => _lootData;
   
        [Header("References")]
        [SerializeField] protected Animator Animator;
        
        protected LootController LootController;
        protected Collider2D Collider;
        protected bool Collected;
        
        // Defined on loot roll (for things such as Gold, which has variable contribution, e.g. Gold loot gives 5 to 9 gold, etc).
        private int _runtimeContribution;
        protected int RuntimeContribution => _runtimeContribution;

        protected virtual void Awake()
        {
            LootController = GetComponent<LootController>();
            Collider = GetComponent<Collider2D>();

            Debug.Assert(LootController != null, $"{name}: LootController missing.", this);
            Debug.Assert(Collider != null, $"{name}: Collider2D missing.", this);
            
            Collider.isTrigger = true;
            
            // Only auto-assign if not manually set
            if (!Animator)
                Animator = GetComponentInChildren<Animator>();
        }

        private void OnEnable()
        {
            Collected = false;
            Collider.enabled = false;

            LootController.CollectableBecameAvailable += HandleCollectableBecameAvailable;
            LootController.MagnetStarted += HandleMagnetStarted;
            LootController.MagnetArrived += HandleMagnetArrived;
        }

        private void OnDisable()
        {
            LootController.CollectableBecameAvailable -= HandleCollectableBecameAvailable;
            LootController.MagnetStarted -= HandleMagnetStarted;
            LootController.MagnetArrived -= HandleMagnetArrived;
        }
        
        public virtual void Initialize(LootSO data, int contribution)
        {
            if (!data)
            {
                Debug.LogError($"{name}: Cannot initialize loot with null LootSO.", this);
                return;
            }
            
            _lootData = data;
            _runtimeContribution = Mathf.Max(0, contribution);
            
            LootController.Initialize(_lootData);
        }

        private void HandleCollectableBecameAvailable()
        {
            if (Collected)
                return;

            if (!_lootData)
                return;

            if (_lootData.collectionMode.IsAutomatic())
            {
                TryCollect(LootCollectionSource.Automatic);
                return;
            }

            if (Collider)
                Collider.enabled = _lootData.collectionMode.RequiresColliderWhenCollectable();
        }

        private void HandleMagnetStarted()
        {
            if (Collected)
                return;

            // Once magnetization starts, the magnet controller owns arrival.
            // Disable trigger interaction to avoid duplicate collection attempts
            // or repeated magnet scans.
            if (Collider)
                Collider.enabled = false;
        }

        private void HandleMagnetArrived()
        {
            TryCollect(LootCollectionSource.Magnet);
        }

        // Existing ICollectable path.
        // Your player-side collision collector can keep calling Collect().
        public virtual void Collect()
        {
            TryCollect(LootCollectionSource.Collision);
        }

        protected bool TryCollect(LootCollectionSource source)
        {
            if (Collected)
                return false;

            if (!_lootData)
                return false;

            if (!LootController)
                return false;

            if (!LootController.HasBecomeCollectable)
                return false;

            if (!CanCollectFromSource(source))
                return false;

            Collected = true;

            if (Collider)
                Collider.enabled = false;

            LootController.MarkCollected();

            OnCollected(source);

            if (Animator)
                Animator.SetTrigger("Interact");
            else
                FinishCollect();

            return true;
        }

        private bool CanCollectFromSource(LootCollectionSource source)
        {
            return source switch
            {
                LootCollectionSource.Collision =>
                    _lootData.collectionMode.AllowsCollisionCollection(),

                LootCollectionSource.Magnet =>
                    _lootData.collectionMode.AllowsMagnetCollection(),

                LootCollectionSource.Automatic =>
                    _lootData.collectionMode.IsAutomatic(),

                _ => false
            };
        }

        protected virtual void OnCollected(LootCollectionSource source)
        {
            // Core hook for derived loot classes.
            // GameLoot overrides this to raise game-specific resource events.
        }

        public virtual void FinishCollect()
        {
            Destroy(gameObject); // Replace with pool return later.
        }
    }
}
