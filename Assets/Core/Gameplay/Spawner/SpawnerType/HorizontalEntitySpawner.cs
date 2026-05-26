using UnityEngine;

namespace Core.Gameplay.Spawner.SpawnerType
{
    public class HorizontalEntitySpawner : EntitySpawner
    {
        [Header("Horizontal Spawner Settings")]
        [SerializeField] private float slotSpacing = 0.15f;
        
        protected override void BuildSpawnSlots()
        {
            // Deterministic slots, alternating left/right
            // Example:  +2, -2, +4, -4, +6, -6 ...

            // Max slots based on max count of entities in any wave
            int maxSlots = GetLargestWaveSize();

            SpawnSlots = new Vector2[maxSlots];

            for (int i = 0; i < maxSlots; i++)
            {
                int ring = (i / 2) + 1;
                float dir = (i % 2 == 0) ? 1f : -1f;
                
                float xOffset = RuntimeController.SpawnData.minSpawnRadius + (ring * slotSpacing);
                
                SpawnSlots[i] = new Vector2(dir * xOffset, 0f);
            }
        }
        
        protected override void BuildSlotOrder()
        {
            int count = SpawnSlots.Length;
            SlotOrder = new int[count];

            for (int i = 0; i < count; i++)
                SlotOrder[i] = i;

            // no shuffle
        }
     
#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            var previewPositions = GetPreviewSpawnPositions();
            if (previewPositions == null)
                return;

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f); // Orange preview

            for (int i = 0; i < previewPositions.Length; i++)
            {
                Gizmos.DrawSphere(previewPositions[i], 0.09f);
                UnityEditor.Handles.Label(
                    previewPositions[i] + Vector3.up * 0.45f,
                    $"Spawn {i + 1}"
                );
            }
        }
        
        protected override Vector3[] GetGizmoSlotPositions()
        {
            if (RuntimeController.SpawnData == null)
                return null;

            int maxSlots = GetLargestWaveSize();
            maxSlots = Mathf.Min(maxSlots, 12); // or any reasonable cap
            
            Vector3[] slots = new Vector3[maxSlots];

            for (int i = 0; i < maxSlots; i++)
            {
                int ring = (i / 2) + 1;
                float dir = (i % 2 == 0) ? 1f : -1f;
                float offset =
                    RuntimeController.SpawnData.minSpawnRadius +
                    (ring * slotSpacing);

                slots[i] = transform.position + Vector3.right * (dir * offset);
            }

            return slots;
        }

        protected override Vector3[] GetPreviewSpawnPositions()
        {
            if (RuntimeController.SpawnData == null)
                return null;

            if (previewWaveIndex < 0 || previewWaveIndex >= RuntimeController.SpawnData.waves.Count)
                return null;

            var wave = RuntimeController.SpawnData.waves[previewWaveIndex];
            int count = wave.entities.Count;

            if (previewSpawnCount > 0)
                count = Mathf.Min(count, previewSpawnCount);

            BuildSpawnSlots();

            Vector3[] positions = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                Vector2 offset = GetSpawnSlotOffset(i);
                positions[i] = (Vector2)transform.position + Vector3.right * offset;
            }

            return positions;
        }
#endif
    }
}
