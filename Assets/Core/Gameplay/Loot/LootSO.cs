using Core.Enum;
using UnityEngine;

namespace Core.Gameplay.Loot
{
    [CreateAssetMenu(fileName = "ResourceSO", menuName = "Loot/Resource")]
    public class LootSO : ScriptableObject
    {
        [Header("Loot")]
        public string displayName;
        public Sprite icon;
        public int defaultContribution;     // TODO: Deprecate this later if we end up never using it
        
        [Header("World Representation")]
        public BaseLoot worldPrefab;
     
        [Header("Movement")]
        public LootPhysicsMode physicsMode = LootPhysicsMode.Lane;
        
        [Header("Collection")]
        public LootCollectionMode collectionMode = LootCollectionMode.CollisionAndMagnet;
        
        [Tooltip("Delay before loot becomes collectable when it does not wait for a ground bounce.")]
        [Min(0f)]
        public float collectableDelay = 0.75f;
       
        [Header("Magnet")]
        [Tooltip("For CollectionMode.Magnet only.")]
        [Min(0.01f)]
        public float baseMagnetSpeed = 6f;
        [Min(0.001f)]
        public float magnetArrivalDistance = 0.05f;
        
        [Header("Bounce")]
        public bool enableBounce;
        public float bounceImpulse = 6f;
        public LayerMask groundMask;
        
        [Header("Launch Settings")]
        public float horizontalForce = 1f;
        public float verticalForce = 2f;
        
        /*
         This is a future improvement when we starte doing Save/Load as the resource reference isn't serialized.
         
        [SerializeField] private string id;

        public string Id => id;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
                id = System.Guid.NewGuid().ToString();
        }
#endif

        in case we have:
        public class InventoryEntry
        {
            public ResourceSO resource;
            public int amount;
        }
        
        for the save/load we actually need:
        [System.Serializable]
        public class InventoryEntrySave
        {
            public string resourceId;
            public int amount;
        }
        
        Might not get this complicated. Leaving it here for reference.
        
        */
    }
}
