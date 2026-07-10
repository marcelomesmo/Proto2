using Core.Gameplay.Loot;
using Core.Services.Manager;
using Game.Loot;
using UnityEngine;

namespace Game.VFX
{
    [RequireComponent(typeof(GameLoot))]
    public sealed class GameLootFloatingTextEmitter : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private FloatingLootVFX floatingLootPrefab;

        [Header("Spawn")]
        [SerializeField] private Vector3 spawnOffset = new(0f, 0.35f, 0f);

        private GameLoot _loot;

        private void Awake()
        {
            _loot = GetComponent<GameLoot>();
        }

        private void OnEnable()
        {
            _loot.GameLootCollected += HandleGameLootCollected;
        }

        private void OnDisable()
        {
            _loot.GameLootCollected -= HandleGameLootCollected;
        }

        private void HandleGameLootCollected(
            GameLoot loot,
            LootCollectionSource source)
        {
            Vector3 spawnPosition = loot.transform.position + spawnOffset;

            var vfx = VFXPoolManager.Instance.Spawn(
                floatingLootPrefab,
                spawnPosition,
                Quaternion.identity);
            
            if (vfx is FloatingLootVFX floatingLoot)
                floatingLoot.Initialize(loot);
        }
    }
}