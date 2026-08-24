using System;
using Game.Entity.Player;
using Game.Services.Save;
using UnityEngine;

namespace Game.Services.Meta
{
    public sealed class PartyManager
    {
        private const int InitialPartySlotIndex = 0;

        private readonly GameSaveManager _saveManager;
        private readonly PlayerLoadoutData _loadout;
        private readonly CharacterDatabase _characterDatabase;

        public event Action OnPartyChanged;
        public event Action<string> OnCharacterUnlocked;
        public event Action<int> OnPartySlotUnlocked;

        public bool HasConfiguredParty =>
            GetAssignedCharacterCount() > 0;

        public PartyManager(
            GameSaveManager saveManager,
            PlayerLoadoutData loadout,
            CharacterDatabase characterDatabase)
        {
            _saveManager = saveManager ??
                throw new ArgumentNullException(nameof(saveManager));

            _loadout = loadout ??
                throw new ArgumentNullException(nameof(loadout));

            _characterDatabase = characterDatabase ??
                throw new ArgumentNullException(
                    nameof(characterDatabase));
        }

        public bool Initialize()
        {
            PartySlotSaveData[] savedSlots =
                _saveManager.Profile.partySlots;

            if (savedSlots == null)
            {
                Debug.LogError(
                    "[PartyManager] Save contains no party slot data.");

                return false;
            }

            if (savedSlots.Length != _loadout.MaxPartySize)
            {
                Debug.LogError(
                    $"[PartyManager] Saved party has " +
                    $"{savedSlots.Length} slots, but PlayerLoadoutData " +
                    $"expects {_loadout.MaxPartySize}.");

                return false;
            }

            _characterDatabase.BuildIndex();
            _loadout.Initialize();

            for (int slotIndex = 0;
                 slotIndex < savedSlots.Length;
                 slotIndex++)
            {
                string characterId =
                    savedSlots[slotIndex].characterId;

                if (string.IsNullOrWhiteSpace(characterId))
                    continue;

                if (!_characterDatabase.TryGet(
                        characterId,
                        out CharacterDefinition definition))
                {
                    Debug.LogError(
                        $"[PartyManager] Character '{characterId}' saved " +
                        $"in slot {slotIndex} was not found in the database.");

                    return false;
                }

                if (!_loadout.TryAssignToSlot(
                        slotIndex,
                        definition))
                {
                    Debug.LogError(
                        $"[PartyManager] Could not assign character " +
                        $"'{characterId}' to runtime slot {slotIndex}.");

                    return false;
                }
            }

            return true;
        }

        public PartyChangeResult TryCompleteInitialCharacterSelection(
            CharacterDefinition definition)
        {
            if (definition == null ||
                definition.baseStats == null)
            {
                return PartyChangeResult.InvalidCharacter;
            }

            if (HasConfiguredParty)
                return PartyChangeResult.PartyAlreadyConfigured;

            PartySlotSaveData initialSlot =
                _saveManager.Profile
                    .partySlots[InitialPartySlotIndex];

            if (!initialSlot.unlocked)
                return PartyChangeResult.SlotLocked;

            string characterId =
                definition.baseStats.characterId;

            if (string.IsNullOrWhiteSpace(characterId))
                return PartyChangeResult.InvalidCharacter;

            bool newlyUnlocked =
                !_saveManager.Profile
                    .unlockedCharacters
                    .Contains(characterId);

            if (newlyUnlocked)
            {
                _saveManager.Profile
                    .unlockedCharacters
                    .Add(characterId);
            }

            initialSlot.characterId = characterId;

            if (!_loadout.TryAssignToSlot(
                    InitialPartySlotIndex,
                    definition))
            {
                initialSlot.characterId = string.Empty;

                if (newlyUnlocked)
                {
                    _saveManager.Profile
                        .unlockedCharacters
                        .Remove(characterId);
                }

                return PartyChangeResult.InvalidCharacter;
            }

            // Unlock and party assignment are persisted together.
            _saveManager.Save();

            if (newlyUnlocked)
                OnCharacterUnlocked?.Invoke(characterId);

            OnPartyChanged?.Invoke();

            return PartyChangeResult.Success;
        }

        private int GetAssignedCharacterCount()
        {
            int count = 0;

            foreach (PartySlotSaveData slot in
                     _saveManager.Profile.partySlots)
            {
                if (!string.IsNullOrWhiteSpace(
                        slot.characterId))
                {
                    count++;
                }
            }

            return count;
        }
    }
}