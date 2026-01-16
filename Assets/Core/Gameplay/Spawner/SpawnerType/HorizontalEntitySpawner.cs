using UnityEngine;

namespace Core.Gameplay.Spawner.SpawnerType
{
    public class HorizontalEntitySpawner : EntitySpawner
    {
        protected override void BuildSpawnSlots()
        {
            // Deterministic slots, alternating left/right
            // Example:  +2, -2, +4, -4, +6, -6 ...

            int maxSlots = 0;
            foreach (var spawnWave in spawnData.spawnList)
                if (spawnWave.entities.Count >= maxSlots)
                    maxSlots = spawnWave.entities.Count;

            //int maxSlots = spawnData.maxSimultaneousEnemies;
            SpawnSlots = new float[maxSlots];

            float step = spawnData.slotSpacing;

            for (int i = 0; i < maxSlots; i++)
            {
                int ring = (i / 2) + 1;
                float dir = (i % 2 == 0) ? 1f : -1f;
                
                float offset =
                    spawnData.minSpawnRadius +
                    (ring * spawnData.slotSpacing);
                
                SpawnSlots[i] = dir * offset;
            }
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
            if (spawnData == null)
                return null;

            int maxSlots = 0;
            foreach (var wave in spawnData.spawnList)
            {
                if (wave.entities.Count > maxSlots)
                    maxSlots = wave.entities.Count;
            }
            maxSlots = Mathf.Min(maxSlots, 12); // or any reasonable cap
            
            Vector3[] slots = new Vector3[maxSlots];

            for (int i = 0; i < maxSlots; i++)
            {
                int ring = (i / 2) + 1;
                float dir = (i % 2 == 0) ? 1f : -1f;
                float offset =
                    spawnData.minSpawnRadius +
                    (ring * spawnData.slotSpacing);

                slots[i] = transform.position + Vector3.right * (dir * offset);
            }

            return slots;
        }

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
                positions[i] = transform.position + Vector3.right * offset;
            }

            return positions;
        }
#endif
    }
}
