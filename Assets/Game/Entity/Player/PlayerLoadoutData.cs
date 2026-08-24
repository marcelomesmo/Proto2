using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Entity.Player
{
    //
    //  Runtime representation of the currently equipped party.
    //
    //  Persistent slot state is owned by GameSave and PartyManager.
    //
    [CreateAssetMenu(fileName = "Player Loadout", menuName = "Game/Player Loadout")]
    public class PlayerLoadoutData : ScriptableObject
    {
        [Header("Party")]
        [SerializeField, Min(1)] private int maxPartySize = 3;
        
        private CharacterDefinition[] _partySlots;
        
        public int MaxPartySize => maxPartySize;
        
        public IReadOnlyList<CharacterDefinition> PartySlots =>
            _partySlots ??
            throw new InvalidOperationException("[PlayerLoadoutData] Loadout has not been initialized.");

        public int CurrentPartySize
        {
            get
            {
                EnsureInitialized();

                int count = 0;

                foreach (CharacterDefinition character in _partySlots)
                {
                    if (character != null)
                        count++;
                }

                return count;
            }
        }
        
        public void Initialize()
        {
            _partySlots =
                new CharacterDefinition[maxPartySize];
        }
        
        public CharacterDefinition GetCharacterAt(int slotIndex)
        {
            EnsureInitialized();
            ValidateSlotIndex(slotIndex);

            return _partySlots[slotIndex];
        }
        
        public bool TryAssignToSlot(
            int slotIndex,
            CharacterDefinition character)
        {
            EnsureInitialized();

            if (character == null)
                return false;

            if (!IsValidSlotIndex(slotIndex))
                return false;

            if (_partySlots[slotIndex] != null)
                return false;

            if (Contains(character))
                return false;

            _partySlots[slotIndex] = character;
            return true;
        }
        
        public bool TryRemoveFromSlot(int slotIndex)
        {
            EnsureInitialized();

            if (!IsValidSlotIndex(slotIndex))
                return false;

            if (_partySlots[slotIndex] == null)
                return false;

            _partySlots[slotIndex] = null;
            return true;
        }

        public bool Contains(CharacterDefinition character)
        {
            EnsureInitialized();

            if (character == null)
                return false;

            foreach (CharacterDefinition assigned in _partySlots)
            {
                if (assigned == character)
                    return true;
            }

            return false;
        }

        private void EnsureInitialized()
        {
            if (_partySlots == null)
            {
                throw new InvalidOperationException(
                    "[PlayerLoadoutData] Loadout has not been initialized.");
            }
        }
        
        private bool IsValidSlotIndex(int slotIndex)
        {
            return slotIndex >= 0 &&
                   slotIndex < _partySlots.Length;
        }

        private void ValidateSlotIndex(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(slotIndex),
                    slotIndex,
                    $"Party slot must be between 0 and " +
                    $"{_partySlots.Length - 1}.");
            }
        }
    }
}
