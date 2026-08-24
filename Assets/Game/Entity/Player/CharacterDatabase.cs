using System.Collections.Generic;
using UnityEngine;

namespace Game.Entity.Player
{
    [CreateAssetMenu(
        fileName = "CharacterDatabase",
        menuName = "Game/Characters/Character Database")]
    public sealed class CharacterDatabase : ScriptableObject
    {
        [SerializeField]
        private List<CharacterDefinition> characters = new();

        private Dictionary<string, CharacterDefinition> _index;

        public IReadOnlyList<CharacterDefinition> Characters =>
            characters;

        public void BuildIndex()
        {
            _index = new Dictionary<string, CharacterDefinition>();

            foreach (CharacterDefinition definition in characters)
            {
                if (definition == null)
                    continue;

                if (definition.baseStats == null)
                {
                    Debug.LogError(
                        $"[CharacterDatabase] Character definition " +
                        $"'{definition.name}' has no BaseEntityStats.");

                    continue;
                }

                string characterId =
                    definition.baseStats.characterId;

                if (string.IsNullOrWhiteSpace(characterId))
                {
                    Debug.LogError(
                        $"[CharacterDatabase] Character definition " +
                        $"'{definition.name}' has no character ID.");

                    continue;
                }

                if (!_index.TryAdd(characterId, definition))
                {
                    Debug.LogError(
                        $"[CharacterDatabase] Duplicate character ID " +
                        $"'{characterId}'.");
                }
            }
        }

        public bool TryGet(
            string characterId,
            out CharacterDefinition definition)
        {
            if (_index == null)
                BuildIndex();

            return _index.TryGetValue(
                characterId,
                out definition);
        }
    }
}