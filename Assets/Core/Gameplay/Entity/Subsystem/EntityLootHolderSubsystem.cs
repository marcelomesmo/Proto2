using Core.Gameplay.Entity.Tags;
using Core.Gameplay.Loot;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Core.Gameplay.Entity.Subsystem
{
    public class EntityLootHolderSubsystem : BaseSubsystem
    {
        [Header("Loot Table")]
        public LootTableSO lootTable;
        
        private bool _lootRolled;
        private Random _rngController;
        
        protected override void OnInitialize()
        {
            _lootRolled = false;
            
            uint seed = (uint)(
                Controller.GetInstanceID() ^
                Time.frameCount ^
                System.Environment.TickCount
            );
            _rngController = new Random(seed);
        }
        
        protected override void OnDeinitialize()
        {
            _lootRolled = false;
        }
        
        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag != Controller.Stats.deadTag)
                return;

            if (_lootRolled)
                return;

            _lootRolled = true;
            RollAndSpawnLoot();
        }

        private void RollAndSpawnLoot()
        {
            if (!lootTable)
            {
                Debug.LogWarning($"[{nameof(EntityLootHolderSubsystem)}] No loot table assigned.", this);
                return;
            }
            
            if (lootTable.entries == null)
                return;
            
            foreach (var entry in lootTable.entries)
            {
                if (!entry.loot)
                    continue;
                
                if (_rngController.NextFloat() > entry.dropChance)
                    continue;

                // Roll amount of items
                int amount = _rngController.NextInt(entry.minAmount, entry.maxAmount + 1);
                
                int minContribution = entry.minContribution;
                int maxContribution = entry.maxContribution;

                for (int i = 0; i < amount; i++)
                {
                    // Roll per item contribution (e.g. Gold loot contributes between 5 and 9 gold).
                    int contribution = _rngController.NextInt(
                        minContribution,
                        maxContribution + 1
                    );
                    
                    SpawnCollectable(entry.loot, contribution);
                }
            }
        }
        
        private void SpawnCollectable(LootSO loot, int contribution)
        {
            //Debug.Log("Spawning resource " + resource.displayName);

            if (!loot.worldPrefab)
            {
                Debug.LogError($"[{nameof(EntityLootHolderSubsystem)}] Loot '{loot.name}' has no world prefab.", this);
                return;
            }
            
            Vector2 start = transform.position;

            BaseLoot spawned = Instantiate(loot.worldPrefab, start, Quaternion.identity);
            
            if (!spawned) 
                return;
            
            spawned.Initialize(loot, contribution);
            
            if (!spawned.TryGetComponent(out LootController lootController))
            {
                Debug.LogError($"Loot prefab '{spawned.name}' is missing LootController.", spawned);
                Destroy(spawned.gameObject);
                return;
            }
            
            float spread = 0.6f; // tweak
            float bias = (_rngController.NextBool() ? -1f : 1f) * spread;
            
            Vector2 directionBias = new Vector2(bias, 1f);
            
            lootController.Launch(
                directionBias,
                loot.horizontalForce,
                loot.verticalForce
            );
        }
    }
}
