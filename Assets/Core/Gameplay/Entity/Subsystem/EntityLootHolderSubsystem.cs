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
            if (tag == Controller.Stats.deadTag)
            {
                if (_lootRolled)
                    return;

                _lootRolled = true;
                RollAndSpawnLoot();
            }
        }

        private void RollAndSpawnLoot()
        {
            foreach (var entry in lootTable.entries)
            {
                if (_rngController.NextFloat() > entry.dropChance)
                    continue;

                int amount = _rngController.NextInt(entry.minAmount, entry.maxAmount + 1);

                for (int i = 0; i < amount; i++)
                    SpawnCollectable(entry.loot);
            }
        }
        
        private void SpawnCollectable(LootSO loot)
        {
            //Debug.Log("Spawning resource " + resource.displayName);

            Vector2 start = transform.position;

            BaseLoot spawned = Instantiate(loot.worldPrefab, start, Quaternion.identity);
            if (!spawned) return;
            
            LootController lootController = spawned.GetComponent<LootController>();
            if (!lootController)
            {
                Debug.LogError("Loot prefab missing LootController");
                return;
            }
            float spread = 0.6f; // tweak
            float bias = (_rngController.NextBool() ? -1f : 1f) * spread;
            
            Vector2 directionBias = new Vector2(bias, 1f);
            
            lootController.Launch(directionBias, spawned.lootData.horizontalForce, spawned.lootData.verticalForce);
        }
    }
}
