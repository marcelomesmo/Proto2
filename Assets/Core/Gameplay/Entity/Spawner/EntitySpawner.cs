using Core.Services.Manager;
using Enemy;
using UnityEngine;

namespace Core.Gameplay.Entity.Spawner
{
    public class EntitySpawner : MonoBehaviour
    {
        public EntitySpawnerData spawnData;

        public bool IsSpawning { get; private set; }
        private float _spawnTimer = 0f;
        private float _waveTimer = 0f;

        private int _spawnedElements = 0;
        private int _spawnedWaves = 0;
        private bool _isWaveFinished = false;
        public bool IsFinishedSpawning => _spawnedWaves >= spawnData.spawnList.Count;

        // Spawn slots
        private float[] _spawnSlots;

        // -----------------------------
        // Start Spawn logic
        // -----------------------------
        
        public void StartSpawning()
        {
            //Debug.Log("[EnemySpawner] Started spawning.");
            IsSpawning = true;

            // restart variables just in case
            _spawnTimer = 0;
            _spawnedElements = 0;
            _waveTimer = 0;
            _spawnedWaves = 0;
            _isWaveFinished = false;

            BuildSpawnSlots();
        }

        private void BuildSpawnSlots()
        {
            // Deterministic slots, alternating left/right
            // Example:  +2, -2, +4, -4, +6, -6 ...

            int maxSlots = 0;
            foreach (var spawnWave in spawnData.spawnList)
                if (spawnWave.prefabs.Count >= maxSlots)
                    maxSlots = spawnWave.prefabs.Count;

            //int maxSlots = spawnData.maxSimultaneousEnemies;
            _spawnSlots = new float[maxSlots];

            float step = spawnData.slotSpacing;

            for (int i = 0; i < maxSlots; i++)
            {
                int ring = (i / 2) + 1;
                float dir = (i % 2 == 0) ? 1f : -1f;
                
                float offset =
                    spawnData.minSpawnRadius +
                    (ring * spawnData.slotSpacing);
                
                _spawnSlots[i] = dir * offset;
            }
        }

        // -----------------------------
        // Timer logic
        // -----------------------------
        
        private void Update()
        {
#if UNITY_EDITOR
            if (UnityEngine.InputSystem.Keyboard.current?.fKey.wasPressedThisFrame == true
                && !IsSpawning)
                StartSpawning();
#endif

            // avoid early init + check if all waves are finished
            if (!IsSpawning || IsFinishedSpawning)
                return;
            
            // update spawn for current wave
            if (!_isWaveFinished)
            {
                UpdateWaveSpawn();
                // update spawn timer for current spawn
                /*var totalEnemiesInThisWave = spawnData.spawnList[_spawnedWaves].enemyPrefabs.Count;
                if (_spawnedElements < totalEnemiesInThisWave)
                {
                    _spawnTimer += Time.deltaTime;
                    // spawn enemy for current wave
                    if (_spawnTimer >= spawnData.spawnDelay)
                    {
                        _spawnTimer = 0f;
                        SpawnWithinRadius(spawnData.spawnList[_spawnedWaves].enemyPrefabs[_spawnedElements]);
                        _spawnedElements++;
                    }
                }
                // finished current wave, start next one
                else
                {
                    _spawnedWaves++;
                    _isWaveFinished = true;
                    //Debug.Log("[EnemySpawner] Finished spawning " + _spawnedElements + " enemies for wave " + _spawnedWaves + ".");
                    //if (IsFinishedSpawning) // TODO: can remove this, debug only
                    //    Debug.Log("[EnemySpawner] Finished spawning.");
                }*/
            }
            // cooldown before next wave
            else
            {
                UpdateWaveCooldown();
                /*_waveTimer += Time.deltaTime;
                if (_waveTimer >= spawnData.waveDelay)
                {
                    _waveTimer = 0f;
                    _spawnedElements = 0;
                    _spawnTimer = 0f;
                    _isWaveFinished = false;
                }*/
            }
        }

        private void UpdateWaveSpawn()
        {
            var wave = spawnData.spawnList[_spawnedWaves];
            int totalEnemies = wave.prefabs.Count;

            if (_spawnedElements >= totalEnemies)
            {
                _spawnedWaves++;
                _isWaveFinished = true;
                return;
            }

            _spawnTimer += Time.deltaTime;

            if (_spawnTimer >= spawnData.spawnDelay)
            {
                _spawnTimer = 0f;

                SpawnEnemy(
                    wave.prefabs[_spawnedElements],
                    totalEnemies,
                    _spawnedElements
                );

                _spawnedElements++;
            }
        }
        
        private void UpdateWaveCooldown()
        {
            _waveTimer += Time.deltaTime;

            if (_waveTimer >= spawnData.waveDelay)
            {
                _waveTimer = 0f;
                _spawnedElements = 0;
                _isWaveFinished = false;
            }
        }

        public void ResetSpawner()
        {
            IsSpawning = false;
        }

        // -----------------------------
        // Spawn logic
        // -----------------------------
        
        /*private void SpawnWithinRadius(GameObject prefab)
        {
            float distance = Random.Range(spawnData.minSpawnRadius, spawnData.spawnRadius);
            float direction = Random.value < 0.5f ? -1f : 1f;
            float offset = direction * distance;

            Vector2 start = transform.position;
            Vector2 end = start + new Vector2(offset, 0f);
            
            //Debug.Log("Spawning enemy at " + end + ".");

            EnemyController enemyObject = EnemyPoolManager.Instance.Spawn(prefab);
            if (!enemyObject)
                return;

            enemyObject.transform.position = end;
        }*/
        
        private void SpawnEnemy(EntityController prefab, int waveSize, int spawnIndex)
        {
            float offset = GetSpawnOffset(waveSize, spawnIndex);

            Vector2 pos = transform.position;
            pos.x += offset;
            
            // TODO: New Spawn.
            
            /*
             var context = new SpawnContext(
                stats: definition.baseStats,
                attackLoadout: definition.initialAttackLoadout,
                owner: gameObject,
                level: 1,
                powerMultiplier: 1f
            );
            
            EntityController enemy = EntityPoolManager.Instance.Spawn(prefab, pos, Quaternion.identity, );
            if (!enemy)
                return;
            */
        }
        
        private float GetSpawnOffset(int waveSize, int spawnIndex)
        {
            // Use fewer slots for smaller waves
            int usableSlots = Mathf.Min(waveSize, _spawnSlots.Length);

            // Push spawns outward as the wave progresses
            int slotIndex = Mathf.Clamp(spawnIndex, 0, usableSlots - 1);

            return _spawnSlots[slotIndex];
        }
        
        // -----------------------------
        // Util
        // -----------------------------
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (spawnData == null)
                return;

            int maxSlots = 6;
            float spacing = spawnData.slotSpacing;

            Vector3 origin = transform.position;

            Gizmos.color = Color.yellow;

            for (int i = 0; i < maxSlots; i++)
            {
                int ring = (i / 2) + 1;
                float dir = (i % 2 == 0) ? 1f : -1f;

                float offsetX = dir * (spawnData.minSpawnRadius + (ring * spacing));

                Vector3 slotPos = origin + new Vector3(offsetX, 0f, 0f);

                // Draw slot marker
                Gizmos.DrawWireSphere(slotPos, 0.25f);

                // Draw line to origin for clarity
                Gizmos.DrawLine(origin, slotPos);

#if UNITY_EDITOR
                // Label slot index (Scene view only)
                UnityEditor.Handles.Label(
                    slotPos + Vector3.up * 0.3f,
                    $"Slot {i}"
                );
#endif
            }
        }
#endif
    }
}