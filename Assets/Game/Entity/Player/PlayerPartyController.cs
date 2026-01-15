using System.Collections.Generic;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Attack;
using Core.Gameplay.Entity.Spawner;
using Core.Services.Manager;
using UnityEngine;

namespace Game.Entity.Player
{
    public class PlayerPartyController : MonoBehaviour
    {
        [SerializeField] private PlayerLoadoutData loadout;
        [SerializeField] private Transform[] spawnPoints;
        
        private readonly List<EntityController> _activeEntities = new();
        public IReadOnlyList<EntityController> ActiveEntities => _activeEntities;

        public PlayerLoadoutData Loadout => loadout;
        
        private void Start()
        {
            SpawnParty();
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

        public void Despawn()
        {
            foreach (var entity in _activeEntities)
                Destroy(entity.gameObject);

            _activeEntities.Clear();
            
            // TODO: Play the end of level flow?
            //ServiceLocator.Get<GameController>().OnGameEnded();
            //SceneLoader.LoadMenu();
        }
    }
}
