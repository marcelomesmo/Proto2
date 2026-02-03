using System.Collections.Generic;
using Core.Gameplay.Entity;
using Game.Entity.Player.Progression;
using Game.Entity.Player.Subsystem;
using UnityEngine;

namespace Game.UI.CharacterHUD
{
    public sealed class CharacterHUDController : MonoBehaviour
    {
        [SerializeField] private CharacterHUDSlot slotPrefab;
        [SerializeField] private Transform slotRoot;

        private readonly Dictionary<string, CharacterHUDSlot> _slots = new();

        // Called once after the player party is spawned.
        public void Initialize(
            IEnumerable<EntityController> characters)
        {
            Clear();
            
            foreach (var character in characters)
            {
                var slot = Instantiate(slotPrefab, slotRoot);
                
                if (!character.TryGetComponent(out CharacterEvolutionSubsystem evolution))
                {
                    Debug.LogError(
                        $"[CharacterHUDController] Missing CharacterEvolutionSubsystem on {character.name}");
                    continue;
                }
                
                slot.Initialize(character, evolution.EvolutionData);
                
                _slots.Add(character.Stats.characterId, slot);
            }
        }

        private void Clear()
        {
            foreach(Transform child in slotRoot)
                Destroy(child.gameObject);
            
            _slots.Clear();
        }
    }
}