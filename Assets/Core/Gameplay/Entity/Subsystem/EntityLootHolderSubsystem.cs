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

        [Header("Force Settings")]
        public bool useForceOverride;
        public float horizontalForceOverride;
        public float verticalForceOverride;
        
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
        
            //float distance = _rngController.NextFloat(minDistance, maxDistance);
            //float direction = _rngController.NextFloat() < 0.5f ? -1f : 1f;
            //float height = _rngController.NextFloat(minHeight, maxHeight);
            //float duration = _rngController.NextFloat(minDuration, maxDuration);

            Vector2 start = transform.position;
            //Vector2 end = start + new Vector2(distance * direction, 0f);

            BaseLoot spawned = Instantiate(loot.worldPrefab, start, Quaternion.identity);
            if (!spawned)
                return;

            Vector2 directionBias = new Vector2(_rngController.NextBool() ? -1 : 1, 0);
            
            if(useForceOverride)
                spawned.Bouncer.LaunchWithForceOverride(directionBias, horizontalForceOverride, verticalForceOverride);
            else
                spawned.Bouncer.Launch(directionBias);
        }
    }
}
