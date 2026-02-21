using System.Collections.Generic;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Spawn;
using Core.Services;
using Core.Services.Manager;
using Core.Services.Meta;
using Core.Upgrades;
using Game.UI.CharacterHUD;
using UnityEngine;

namespace Game.Entity.Player
{
    public class PlayerPartyController : MonoBehaviour
    {
        [SerializeField] private CastleDefinition castleDefinition;
        [SerializeField] private Transform castleSpawnPoint;
        
        [SerializeField] private PlayerLoadoutData loadout;
        [SerializeField] private Transform[] spawnPoints;
        
        [SerializeField] private CharacterHUDController characterHUD;
        
        private EntityController _castle;
        private PlayerCastleController _castleController;
        private readonly List<EntityController> _activeEntities = new();
        public IReadOnlyList<EntityController> ActiveEntities => _activeEntities;

        public PlayerLoadoutData Loadout => loadout;
        
        private void Start()
        {
            SpawnCastle();
            SpawnParty();
            ApplyUpgrades();
            InitializeHUD();
        }

        private void SpawnCastle()
        {
            var definition = castleDefinition;
            var spawn = castleSpawnPoint;
            
            var context = new SpawnContext(
                stats: definition.baseStats,
                attackLoadout: null, // Castle has no attacks.
                // Later we can create a factory: SpawnContext.ForPassiveEntity(...) SpawnContext.ForCombatEntity(...)
                owner: gameObject,
                level: 1,
                powerMultiplier: 1f
            );
            
            _castle = EntityPoolManager.Instance.Spawn(
                definition.prefab,
                spawn.position,
                Quaternion.identity,
                context
            );
            
            _castleController = _castle.GetComponent<PlayerCastleController>();
            
            Debug.Assert(_castleController != null,
                "Castle prefab must have PlayerCastleController.");
            
            _castleController.CastleDestroyed += OnDefeat;
        }

        private void SpawnParty()
        {
            for (int i = 0; i < loadout.SelectedCharacters.Count; i++)
            {
                var def = loadout.SelectedCharacters[i];
                var spawn = spawnPoints[i];

                SpawnCharacter(def, spawn);
            }
        }
        
        public void SpawnCharacter(CharacterDefinition definition, Transform spawnPoint)
        {
            var context = new SpawnContext(
                stats: definition.baseStats,
                attackLoadout: definition.initialAttackLoadout,
                owner: gameObject,
                level: 1,
                powerMultiplier: 1f
            );
            
            var entity = EntityPoolManager.Instance.Spawn(
                definition.prefab,
                spawnPoint.position,
                Quaternion.identity,
                context
            );

            _activeEntities.Add(entity);
        }
        
        #region Upgrades Initialize
        
        private void ApplyUpgrades()
        {
            var upgradeManager =
                ServiceLocator.Get<GameController>()?.UpgradeManager;

            if (upgradeManager == null)
                return;

            foreach (var entity in _activeEntities)
            {
                if (!entity)
                    continue;

                ApplyUpgradesToEntity(entity, upgradeManager);
            }
        }
        
        private void ApplyUpgradesToEntity(
            EntityController entity,
            UpgradeManager manager)
        {
            foreach (var upgrade in manager.ActiveUpgrades)
            {
                var context = new UpgradeContext(entity);

                foreach (var effect in upgrade.Effects)
                {
                    effect.Apply(context);
                }
            }
        }
        
        #endregion
        
        private void InitializeHUD()
        {
            if (!characterHUD)
            {
                Debug.LogWarning("[PlayerPartyController] No CharacterHUDController assigned.");
                return;
            }

            characterHUD.Initialize(_activeEntities);
        }

        public void OnDefeat()
        {
            Despawn();
        }
        private void Despawn()
        {
            EntityPoolManager.Instance.Despawn(_castle);
            
            foreach (var entity in _activeEntities)
                EntityPoolManager.Instance.Despawn(entity);

            // Clear Entity references
            _activeEntities.Clear();
            
            // Clear Castle references
            if (_castleController != null)
            {
                _castleController.CastleDestroyed -= OnDefeat;
                _castleController = null;
            }
            _castle = null;
            
            // End of Level -> Defeat
            ServiceLocator.Get<GameController>().OnGameDefeat();
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (spawnPoints == null || spawnPoints.Length <= 0)
                return;
            
            Gizmos.color = new Color(0.5f, 0.8f, 0.5f, 0.75f);

            for (int i = 0; i < spawnPoints.Length; i++)
            {
                Gizmos.DrawWireSphere(spawnPoints[i].position, 0.2f);
                
                UnityEditor.Handles.Label(
                    spawnPoints[i].position + Vector3.up * 0.25f,
                    $"Char {i + 1}"
                );
            }
        }     
#endif
    }
}
