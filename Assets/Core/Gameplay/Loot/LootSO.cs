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
        public int contribution;
        
        [Header("World Representation")]
        public BaseLoot worldPrefab;
     
        [Header("Loot behavior")]
        public LootPhysicsMode physicsMode = LootPhysicsMode.Lane;
        public float baseMagnetSpeed = 6f;
        public float magnetStartDelay = 0.75f;
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
