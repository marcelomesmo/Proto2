using UnityEngine;

namespace Core.Gameplay.Spawner.SpawnerType
{
    public class LaneEntitySpawner : EntitySpawner
    {
        [Header("Lane Spawner Settings")]
        [SerializeField, Min(1)] private int maxLaneSlots = 5;
        [SerializeField] private float slotSpacing = 0.15f;
        
        protected override void BuildSpawnSlots()
        {
            // Vertical deterministic slots centered on spawner
            // Example (Y offsets): +1, -1, +2, -2, +3, -3 ...

            SpawnSlots = new Vector2[maxLaneSlots];
            
            float half = (maxLaneSlots - 1) * 0.5f;
            
            for (int i = 0; i < maxLaneSlots; i++)
            {
                float yOffset = (i - half) * slotSpacing;

                SpawnSlots[i] = new Vector2(0f, yOffset);   // builds yOffset vector, other implements can be radial, sin/cos, etc
            }
        }

        protected override void BuildSlotOrder()
        {
            int count = SpawnSlots.Length;
            SlotOrder = new int[count];

            for (int i = 0; i < count; i++)
                SlotOrder[i] = i;

            // Fisher–Yates shuffle
            for (int i = count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (SlotOrder[i], SlotOrder[j]) = (SlotOrder[j], SlotOrder[i]);
            }
        }
        
#if UNITY_EDITOR
        protected override Vector3[] GetPreviewSpawnPositions()
        {
            if (spawnData == null)
                return null;

            if (previewWaveIndex < 0 || previewWaveIndex >= spawnData.spawnList.Count)
                return null;

            var wave = spawnData.spawnList[previewWaveIndex];
            int count = wave.entities.Count;

            if (previewSpawnCount > 0)
                count = Mathf.Min(count, previewSpawnCount);

            BuildSpawnSlots();
            
            // DO NOT call BuildSlotOrder() for preview if you don't want the shuffled positions to be displayed.
            // Use a deterministic order instead
            /*Vector3[] positions = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                int slotIndex = i % SpawnSlots.Length;
                Vector2 offset = SpawnSlots[slotIndex];
                positions[i] = (Vector2)transform.position + offset;
            }*/

            BuildSlotOrder();
            
            Vector3[] positions = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = GetSpawnSlotOffset(i);
                positions[i] = (Vector2)transform.position + offset;
            }

            return positions;
        }

        protected override Vector3[] GetGizmoSlotPositions()
        {
            if (spawnData == null)
                return null;
            
            Vector3[] slots = new Vector3[maxLaneSlots];

            float spacing = slotSpacing;
            float half = (maxLaneSlots - 1) * 0.5f;
            
            for (int i = 0; i < maxLaneSlots; i++)
            {
                float offset = (i - half) * spacing;
                slots[i] = transform.position + Vector3.up * offset;
            }

            return slots;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            var previewPositions = GetPreviewSpawnPositions();
            if (previewPositions == null)
                return;

            Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.9f); // Portal-blue preview

            for (int i = 0; i < previewPositions.Length; i++)
            {
                Gizmos.DrawSphere(previewPositions[i], 0.09f);
                UnityEditor.Handles.Label(
                    previewPositions[i] + Vector3.right * 0.4f,
                    $"Spawn {i + 1}"
                );
            }
        }
#endif
    }
}
