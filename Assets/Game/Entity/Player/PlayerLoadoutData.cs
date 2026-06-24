using System.Collections.Generic;
using UnityEngine;

namespace Game.Entity.Player
{
    //
    //  Represents the current selected party loadout.
    //
    //  Built during run-time.
    //
    [CreateAssetMenu(fileName = "Player Loadout", menuName = "Game/Player Loadout")]
    public class PlayerLoadoutData : ScriptableObject
    {
        [Header("Party")]
        [SerializeField] private int maxPartySize = 5;
        [SerializeField] private List<CharacterDefinition> selectedCharacters;

        public IReadOnlyList<CharacterDefinition> SelectedCharacters =>
            selectedCharacters;
        public int MaxPartySize => maxPartySize;
        public int CurrentPartySize => selectedCharacters.Count;

        public bool CanAdd(CharacterDefinition character)
        {
            if (character == null)
                return false;

            if (selectedCharacters.Contains(character))
                return false;

            return selectedCharacters.Count < maxPartySize;
        }
        
        public bool AddToParty(CharacterDefinition character)
        {
            if (!CanAdd(character))
                return false;

            selectedCharacters.Add(character);
            return true;
        }

        public void RemoveFromParty(CharacterDefinition character)
        {
            selectedCharacters.Remove(character);
        }

        public void ClearParty()
        {
            selectedCharacters.Clear();
        }
        
        public bool Contains(CharacterDefinition character)
        {
            return selectedCharacters.Contains(character);
        }
    }
}
