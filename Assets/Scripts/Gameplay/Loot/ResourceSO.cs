using Gameplay.Loot;
using UnityEngine;

namespace Gameplay.Collectables
{
    [CreateAssetMenu(fileName = "ResourceSO", menuName = "Loot/Resource")]
    public class ResourceSO : ScriptableObject
    {
        public string displayName;
        public Sprite icon;
        public int contribution;
        
        [Header("World Representation")]
        public BaseCollectable worldPrefab;
        
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
