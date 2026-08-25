using System;
using System.Collections.Generic;
using Game.Entity.Player;
using Game.Services.Save;
using UnityEngine;

namespace Game.Services.Meta
{
    public sealed class PartyManager
    {
        private const int InitialPartySlotIndex = 0;
        private const float CharacterRestDurationSeconds = 30f;

        private readonly GameSaveManager _saveManager;
        private readonly PlayerLoadoutData _loadout;
        private readonly CharacterDatabase _characterDatabase;

        public event Action OnPartyChanged;
        public event Action<string> OnCharacterUnlocked;
        public event Action<int> OnPartySlotUnlocked;
        public event Action<int, CharacterDefinition> OnPartySlotChanged;

        public bool HasConfiguredParty => GetAssignedCharacterCount() > 0;
        
        private readonly Dictionary<string, float> _characterRestUntil = new();

        public PartyManager(GameSaveManager saveManager, PlayerLoadoutData loadout, CharacterDatabase characterDatabase)
        {
            _saveManager = saveManager ?? throw new ArgumentNullException(nameof(saveManager));

            _loadout = loadout ?? throw new ArgumentNullException(nameof(loadout));

            _characterDatabase = characterDatabase ?? throw new ArgumentNullException(nameof(characterDatabase));
        }

        public bool Initialize()
        {
            PartySlotSaveData[] savedSlots = _saveManager.Profile.partySlots;

            if (savedSlots == null)
            {
                Debug.LogError("[PartyManager] Save contains no party slot data.");
                return false;
            }

            if (savedSlots.Length != _loadout.MaxPartySize)
            {
                Debug.LogError($"[PartyManager] Saved party has {savedSlots.Length} slots, but PlayerLoadoutData expects {_loadout.MaxPartySize}.");
                return false;
            }

            _characterDatabase.BuildIndex();
            _loadout.Initialize();

            for (int slotIndex = 0; slotIndex < savedSlots.Length; slotIndex++)
            {
                string characterId = savedSlots[slotIndex].characterId;

                if (string.IsNullOrWhiteSpace(characterId))
                    continue;

                if (!_characterDatabase.TryGet(characterId, out CharacterDefinition definition))
                {
                    Debug.LogError($"[PartyManager] Character '{characterId}' saved in slot {slotIndex} was not found in the database.");
                    return false;
                }

                if (!_loadout.TryAssignToSlot(slotIndex, definition))
                {
                    Debug.LogError($"[PartyManager] Could not assign character '{characterId}' to runtime slot {slotIndex}.");
                    return false;
                }
            }

            return true;
        }

        // --------------------------------------------------
        // Initial Character Selection
        // --------------------------------------------------
        #region Initial Character Selection
        
        public PartyChangeResult TryCompleteInitialCharacterSelection(CharacterDefinition definition)
        {
            if (definition == null || definition.baseStats == null)
                return PartyChangeResult.InvalidCharacter;

            if (HasConfiguredParty)
                return PartyChangeResult.PartyAlreadyConfigured;

            PartySlotSaveData initialSlot = _saveManager.Profile.partySlots[InitialPartySlotIndex];

            if (!initialSlot.unlocked)
                return PartyChangeResult.SlotLocked;

            string characterId = definition.baseStats.characterId;

            if (string.IsNullOrWhiteSpace(characterId))
                return PartyChangeResult.InvalidCharacter;

            bool newlyUnlocked = !_saveManager.Profile.unlockedCharacters.Contains(characterId);

            if (newlyUnlocked)
                _saveManager.Profile.unlockedCharacters.Add(characterId);

            initialSlot.characterId = characterId;

            if (!_loadout.TryAssignToSlot(InitialPartySlotIndex, definition))
            {
                initialSlot.characterId = string.Empty;

                if (newlyUnlocked)
                    _saveManager.Profile.unlockedCharacters.Remove(characterId);

                return PartyChangeResult.InvalidCharacter;
            }

            // Unlock and party assignment are persisted together.
            _saveManager.Save();

            if (newlyUnlocked)
                OnCharacterUnlocked?.Invoke(characterId);

            OnPartyChanged?.Invoke();

            return PartyChangeResult.Success;
        }
        
        #endregion

        private int GetAssignedCharacterCount()
        {
            int count = 0;

            foreach (PartySlotSaveData slot in _saveManager.Profile.partySlots)
            {
                if (!string.IsNullOrWhiteSpace(slot.characterId))
                    count++;
            }

            return count;
        }
        
        // --------------------------------------------------
        // UI and External Queries
        // --------------------------------------------------
        #region UI and External Queries
        
        public int SlotCount => _saveManager.Profile.partySlots.Length;

        public bool IsSlotUnlocked(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
                return false;

            return _saveManager.Profile.partySlots[slotIndex].unlocked;
        }

        public CharacterDefinition GetCharacterAt(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
                return null;

            return _loadout.GetCharacterAt(slotIndex);
        }

        public bool IsCharacterUnlocked(CharacterDefinition definition)
        {
            if (!TryGetCharacterId(definition, out string characterId))
                return false;

            return _saveManager.IsCharacterUnlocked(characterId);
        }

        public int FindAssignedSlot(CharacterDefinition definition)
        {
            if (!TryGetCharacterId(definition, out string characterId))
                return -1;

            PartySlotSaveData[] slots = _saveManager.Profile.partySlots;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].characterId == characterId)
                    return i;
            }

            return -1;
        }

        public bool IsCharacterResting(CharacterDefinition definition)
        {
            return GetCharacterRestRemaining(definition) > 0f;
        }

        public float GetCharacterRestRemaining(CharacterDefinition definition)
        {
            if (!TryGetCharacterId(definition, out string characterId))
                return 0f;

            if (!_characterRestUntil.TryGetValue(characterId, out float restUntil))
                return 0f;

            float remaining = restUntil - Time.unscaledTime;

            if (remaining > 0f)
                return remaining;

            _characterRestUntil.Remove(characterId);

            return 0f;
        }
        
        #endregion
        
        
        // --------------------------------------------------
        // Character Purchase
        // --------------------------------------------------
        #region Character Purchase
        
        public PartyChangeResult TryPurchaseCharacter(CharacterDefinition definition)
        {
            if (!TryGetCharacterId(definition, out string characterId))
                return PartyChangeResult.InvalidCharacter;

            if (_saveManager.IsCharacterUnlocked(characterId))
                return PartyChangeResult.CharacterAlreadyUnlocked;

            int cost = Mathf.Max(0, definition.unlockCost);

            // UI will also display affordability, but the domain layer validates it independently.
            if (_saveManager.Gold < cost)
                return PartyChangeResult.InsufficientGold;

            if (cost > 0 && !_saveManager.SpendGold(cost))
                return PartyChangeResult.InsufficientGold;

            _saveManager.Profile.unlockedCharacters.Add(characterId);

            _saveManager.Save();

            OnCharacterUnlocked?.Invoke(characterId);

            return PartyChangeResult.Success;
        }

        #endregion
        
        
        // --------------------------------------------------
        // Character-to-Slot Assignment/Replacement
        // --------------------------------------------------
        #region Character-to-Slot Assignment/Replacement
        
        public PartyChangeResult TryAssignCharacterToSlot(CharacterDefinition definition, int slotIndex)
        {
            if (!TryGetCharacterId(definition, out string characterId))
                return PartyChangeResult.InvalidCharacter;

            if (!IsValidSlotIndex(slotIndex))
                return PartyChangeResult.InvalidPartySize;

            if (!IsSlotUnlocked(slotIndex))
                return PartyChangeResult.SlotLocked;

            if (!_saveManager.IsCharacterUnlocked(characterId))
                return PartyChangeResult.CharacterLocked;

            if (FindAssignedSlot(definition) >= 0)
                return PartyChangeResult.CharacterAlreadyAssigned;

            if (IsCharacterResting(definition))
                return PartyChangeResult.CharacterResting;

            CharacterDefinition previousCharacter = _loadout.GetCharacterAt(slotIndex);

            // Replace runtime loadout state.
            if (previousCharacter != null)
            {
                if (!_loadout.TryRemoveFromSlot(slotIndex))
                {
                    Debug.LogError($"[PartyManager] Failed to remove current character from slot {slotIndex}.");
                    return PartyChangeResult.InvalidCharacter;
                }
            }

            if (!_loadout.TryAssignToSlot(slotIndex, definition))
            {
                // Restore the old runtime assignment if something unexpected prevented the replacement.
                if (previousCharacter != null)
                    _loadout.TryAssignToSlot(slotIndex, previousCharacter);

                Debug.LogError($"[PartyManager] Failed to assign character '{characterId}' to slot {slotIndex}.");
                return PartyChangeResult.InvalidCharacter;
            }

            // Persist the new assignment.
            _saveManager.Profile.partySlots[slotIndex].characterId = characterId;

            // A character that leaves battle enters its resting period.
            if (previousCharacter != null)
                StartCharacterRest(previousCharacter);

            _saveManager.Save();

            // Notify live combat first.
            OnPartySlotChanged?.Invoke(slotIndex, definition);

            // General UI refresh notification.
            OnPartyChanged?.Invoke();

            return PartyChangeResult.Success;
        }
        
        #endregion
        
        
        // --------------------------------------------------
        // Helpers
        // --------------------------------------------------
        #region Helpers
        
        private void StartCharacterRest(CharacterDefinition definition)
        {
            if (!TryGetCharacterId(definition, out string characterId))
                return;

            _characterRestUntil[characterId] = Time.unscaledTime + CharacterRestDurationSeconds;
        }

        private bool IsValidSlotIndex(int slotIndex)
        {
            PartySlotSaveData[] slots = _saveManager.Profile.partySlots;

            return slots != null &&
                   slotIndex >= 0 &&
                   slotIndex < slots.Length;
        }

        private static bool TryGetCharacterId(CharacterDefinition definition, out string characterId)
        {
            characterId = null;

            if (definition == null || definition.baseStats == null)
                return false;

            characterId = definition.baseStats.characterId;

            return !string.IsNullOrWhiteSpace(characterId);
        }
        
        #endregion
    }
}