using System.Collections.Generic;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Spawn;
using Core.Services;
using Core.Services.Manager;
using Core.Services.Meta;
using Core.Services.Save;
using Core.Upgrades;
using Game.Entity.Player.Subsystem;
using Game.Services.Meta;
using Game.Services.Save;
using UnityEngine;

namespace Game.Entity.Player
{
    public class PlayerPartyController : MonoBehaviour
    {
        [SerializeField] private CastleDefinition castleDefinition;
        [SerializeField] private Transform castleSpawnPoint;
        
        [SerializeField] private PlayerLoadoutData loadout;
        [SerializeField] private Transform[] spawnPoints;
        
        private EntityController _castle;
        private PlayerCastleController _castleController;
        
        private EntityController[] _slotEntities;   // High-confidence reference to party slots
        private PartyManager _partyManager;
        
        private readonly List<EntityController> _activeEntities = new();    // Low-confidence reference (compact list for operations that apply to every active character)
        public IReadOnlyList<EntityController> ActiveEntities => _activeEntities;

        public PlayerLoadoutData Loadout => loadout;
        
        private bool _initialized;
        
        // Explicit entrypoint (called by MatchSceneController).
        // Spawns party, applies upgrades, initializes HUD.
        public void Initialize()
        {
            if (_initialized)
            {
                Debug.LogWarning("[PlayerPartyController] BeginMatchParty called more than once.", this);
                return;
            }
            
            _initialized = true;
            
            _slotEntities = new EntityController[loadout.MaxPartySize];

            SpawnCastle();
            SpawnParty();

            ApplyUpgradesToParty();
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
            IReadOnlyList<CharacterDefinition> partySlots = loadout.PartySlots;
            
            // Small spawn-point sanity-check
            int slotCount = Mathf.Min(partySlots.Count, spawnPoints.Length);
            
            for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
            {
                var definition = partySlots[slotIndex];
                if (definition == null)
                    continue;
                
                var spawnPoint = spawnPoints[slotIndex];
                if (spawnPoint == null)
                {
                    Debug.LogError($"[PlayerPartyController] Spawn point {slotIndex} is not assigned.", this);
                    continue;
                }

                EntityController entity = SpawnCharacterEntity(definition, spawnPoint);

                if (entity == null)
                    continue;

                _slotEntities[slotIndex] = entity;
                _activeEntities.Add(entity);
            }
        }
        
        public void SpawnCharacter(CharacterDefinition definition, Transform spawnPoint)
        {
            EntityController entity = SpawnCharacterEntity(definition, spawnPoint);

            if (entity != null)
                _activeEntities.Add(entity);
        }
        
        private EntityController SpawnCharacterEntity(CharacterDefinition definition, Transform spawnPoint)
        {
            if (definition == null || spawnPoint == null)
                return null;
            
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

            if (entity == null)
                return null;
            
            // Restore saved state for this unit
            RestoreCharacterProgression(entity, definition);

            return entity;
        }
        
        private void OnDestroy()
        {
            UnbindPartyManager();
        }
        
        #region Characters Initialize
        
        private void RestoreCharacterProgression(
            EntityController entity,
            CharacterDefinition definition)
        {
            if (entity == null || definition == null || definition.baseStats == null)
                return;

            var saveManager = ServiceLocator.Get<ISaveManager>() as GameSaveManager;

            if (saveManager == null)
            {
                Debug.LogError("[PlayerPartyController] GameSaveManager service not found.", this);
                return;
            }

            string characterId = definition.baseStats.characterId;

            // Load saved data
            int savedXp = saveManager.GetCharacterXp(characterId);

            // Restore level
            if (!entity.TryGetComponent(out CharacterLevelSubsystem levelSubsystem))
                return;
            
            levelSubsystem.RestoreProgression(savedXp);

            // Restore evolution
            if (!entity.TryGetComponent(out CharacterEvolutionSubsystem evolutionSubsystem))
                return;
            
            evolutionSubsystem.RestoreForLevel(levelSubsystem.Level);
        }
        
        #endregion
        
        #region Upgrades Initialize
        
        private void ApplyUpgradesToParty()
        {
            var upgradeManager =
                ServiceLocator.Get<GameController>()?.UpgradeRuntimeManager;

            if (upgradeManager == null)
                return;

            foreach (var entity in _activeEntities)
            {
                if (!entity)
                    continue;

                ApplyUpgradesToEntity(entity, upgradeManager);
            }
            
            ApplyUpgradesToEntity(_castle, upgradeManager);
        }
        
        private void ApplyUpgradesToEntity(
            EntityController entity,
            UpgradeRuntimeManager upgradeRuntimeManager)
        {
            var context = new UpgradeContext(entity);
            
            foreach (var runtime in upgradeRuntimeManager.ActiveUpgrades)
            {
                var def = runtime.Definition;
                int level = runtime.Level;

                if (!def || level <= 0)
                    continue;
                
                foreach (var effect in def.Effects)
                {
                    if (!effect)
                        continue;

                    if (!effect.CanApply(context))
                        continue;
                    
                    effect.Apply(context, level);
                }
            }
        }
        
        // Apply Upgrades to Characters joining the Party
        private void ApplyCurrentUpgradesToEntity(EntityController entity)
        {
            if (entity == null)
                return;

            UpgradeRuntimeManager upgradeManager = ServiceLocator.Get<GameController>()?.UpgradeRuntimeManager;

            if (upgradeManager == null)
                return;

            ApplyUpgradesToEntity(entity, upgradeManager);
        }
        
        #endregion

        public void OnDefeat()
        {
            DisablePartyCombat();
            
            // Notify the controller
            ServiceLocator.Get<GameController>()?.OnGameDefeat();
        }
        private void DisablePartyCombat()
        {
            foreach (var entity in _activeEntities)
            {
                if (!entity)
                    continue;

                entity.Tags.AddTag(entity.Stats.matchEndedTag);
                // TODO: Add Animation play here (for character defeat, if any).
            }
        }
        public void EndAttempt()
        {
            if (_castleController != null)
            {
                _castleController.CastleDestroyed -= OnDefeat;
            }

            //characterHUD?.Clear();
            
            _activeEntities.Clear();
            
            _slotEntities = null;

            _castle = null;
            _castleController = null;

            _initialized = false;
        }
        
        public void BindPartyManager(PartyManager partyManager)
        {
            if (_partyManager == partyManager)
                return;

            UnbindPartyManager();

            _partyManager = partyManager;

            if (_partyManager != null)
                _partyManager.OnPartySlotChanged += HandlePartySlotChanged;
        }

        private void UnbindPartyManager()
        {
            if (_partyManager == null)
                return;

            _partyManager.OnPartySlotChanged -= HandlePartySlotChanged;

            _partyManager = null;
        }
        
        private void HandlePartySlotChanged(int slotIndex, CharacterDefinition definition)
        {
            // The persistent/runtime loadout can change even before an attempt exists (e.g. FTUE selection).
            // In that case normal Initialize() will spawn the correct party.
            if (!_initialized)
                return;

            if (_slotEntities == null || slotIndex < 0 || slotIndex >= _slotEntities.Length)
            {
                Debug.LogError($"[PlayerPartyController] Invalid runtime party slot {slotIndex}.", this);
                return;
            }

            if (slotIndex >= spawnPoints.Length || spawnPoints[slotIndex] == null)
            {
                Debug.LogError($"[PlayerPartyController] Missing spawn point for party slot {slotIndex}.", this);
                return;
            }

            EntityController previousEntity = _slotEntities[slotIndex];

            if (previousEntity != null)
            {
                _activeEntities.Remove(previousEntity);

                EntityPoolManager.Instance.Despawn(previousEntity);

                _slotEntities[slotIndex] = null;
            }

            if (definition == null)
                return;

            EntityController replacement = SpawnCharacterEntity(definition, spawnPoints[slotIndex]);

            if (replacement == null)
                return;

            _slotEntities[slotIndex] = replacement;

            _activeEntities.Add(replacement);

            ApplyCurrentUpgradesToEntity(replacement);
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
