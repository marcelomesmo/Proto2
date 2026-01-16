using UnityEngine;

namespace Core.Gameplay.Spawner.SpawnerType
{
    public class LaneEntitySpawner : EntitySpawner
    {
        [Header("Lane Spawner Settings")]
        [SerializeField, Min(1)] private int maxLaneSlots = 5;
        
        protected override void BuildSpawnSlots()
        {
            // Vertical deterministic slots centered on spawner
            // Example (Y offsets): +1, -1, +2, -2, +3, -3 ...

            SpawnSlots = new float[maxLaneSlots];
            
            float spacing = spawnData.slotSpacing;
            float half = (maxLaneSlots - 1) * 0.5f;
            
            for (int i = 0; i < maxLaneSlots; i++)
            {
                float offset = (i - half) * spacing;

                SpawnSlots[i] = offset;
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

            Vector3[] positions = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                float offset = GetSpawnSlotOffset(count, i);
                positions[i] = transform.position + Vector3.up * offset;
            }

            return positions;
        }

        protected override Vector3[] GetGizmoSlotPositions()
        {
            if (spawnData == null)
                return null;
            
            Vector3[] slots = new Vector3[maxLaneSlots];

            float spacing = spawnData.slotSpacing;
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
