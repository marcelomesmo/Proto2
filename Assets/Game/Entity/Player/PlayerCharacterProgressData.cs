using System.Collections.Generic;
using UnityEngine;

namespace Game.Entity.Player
{
    [CreateAssetMenu(fileName = "Player Character Progress Data", menuName = "Game/Player Character Progress Data")]
    public class PlayerCharacterProgressData : ScriptableObject
    {
        [SerializeField] private List<string> unlockedCharacterIds = new();

        public bool IsUnlocked(CharacterDefinition character)
        {
            return unlockedCharacterIds.Contains(character.characterId);
        }

        public void Unlock(CharacterDefinition character)
        {
            if (!unlockedCharacterIds.Contains(character.characterId))
                unlockedCharacterIds.Add(character.characterId);
        }
    }
}
