using System;
using Core.EventChannels;
using Core.EventChannels.Payloads;
using Game.Entity.Player;
using Game.Entity.Player.Progression.Util;
using Game.Entity.Player.Subsystem;
using Game.Services.Meta;
using Game.Services.Save;
using UnityEngine;

namespace Game.UI.Party
{
    public sealed class PartyPanelController : MonoBehaviour
    {
        [Header("Party Slots")]
        [SerializeField] private PartySlotView[] partySlots;

        [Header("Roster")]
        [SerializeField] private CharacterRosterItemView[] characterItems;

        [Header("Purchase")]
        [SerializeField] private CharacterPurchasePopup purchasePopup;

        [Header("Character Progression Events")]
        [SerializeField] private CharacterLevelChangedEventChannelSO levelChangedEvent;
        [SerializeField] private CharacterStageChangedEventChannelSO stageChangedEvent;
        
        private PartyManager _partyManager;
        private GameSaveManager _saveManager;

        private int _selectedSlotIndex = -1;
        private bool _assignmentMode;

        private bool _initialized;

        // --------------------------------------------------
        // Initialization
        // --------------------------------------------------

        public void Initialize(PartyManager partyManager, GameSaveManager saveManager)
        {
            if (_initialized)
                return;

            _partyManager = partyManager ?? throw new ArgumentNullException(nameof(partyManager));

            _saveManager = saveManager ?? throw new ArgumentNullException(nameof(saveManager));

            InitializeSlotViews();

            BindRosterViews();

            purchasePopup.BuyClicked += HandlePurchaseRequested;

            _partyManager.OnPartyChanged += HandlePartyChanged;
            _partyManager.OnCharacterUnlocked += HandleCharacterUnlocked;
            _partyManager.OnPartySlotUnlocked += HandlePartySlotUnlocked;

            _saveManager.OnGoldChanged += HandleGoldChanged;
            
            levelChangedEvent.OnEventRaised += HandleCharacterLevelChanged;
            stageChangedEvent.OnEventRaised += HandleCharacterStageChanged;

            _initialized = true;

            RefreshAll();
        }

        private void InitializeSlotViews()
        {
            if (partySlots.Length != _partyManager.SlotCount)
            {
                Debug.LogError($"[PartyPanelController] UI contains {partySlots.Length} party slots but PartyManager contains {_partyManager.SlotCount}.", this);
            }

            for (int i = 0; i < partySlots.Length; i++)
            {
                PartySlotView view = partySlots[i];

                if (view == null)
                    continue;

                view.Initialize(i);

                view.Clicked += HandlePartySlotClicked;
                view.ActionClicked += HandleSlotActionClicked;
            }
        }

        private void BindRosterViews()
        {
            foreach (CharacterRosterItemView item in characterItems)
            {
                if (item == null)
                    continue;

                item.Clicked += HandleRosterCharacterClicked;
            }
        }

        // --------------------------------------------------
        // Unity Lifecycle
        // --------------------------------------------------

        private void OnEnable()
        {
            if (_initialized)
                RefreshAll();
        }

        private void OnDisable()
        {
            if (!_initialized)
                return;

            _selectedSlotIndex = -1;
            _assignmentMode = false;

            purchasePopup.Hide();
        }

        private void Update()
        {
            if (!_initialized)
                return;

            // Rest timers are runtime-only, so refresh only the roster availability state each frame.
            RefreshRosterRuntimeState();
        }

        private void OnDestroy()
        {
            if (!_initialized)
                return;

            foreach (PartySlotView view in partySlots)
            {
                if (view != null)
                {
                    view.Clicked -= HandlePartySlotClicked;
                    view.ActionClicked -= HandleSlotActionClicked;
                }
            }

            foreach (CharacterRosterItemView item in characterItems)
            {
                if (item != null)
                {
                    item.Clicked -= HandleRosterCharacterClicked;
                }
            }

            purchasePopup.BuyClicked -= HandlePurchaseRequested;

            _partyManager.OnPartyChanged -= HandlePartyChanged;
            _partyManager.OnCharacterUnlocked -= HandleCharacterUnlocked;
            _partyManager.OnPartySlotUnlocked -= HandlePartySlotUnlocked;

            _saveManager.OnGoldChanged -= HandleGoldChanged;
            
            levelChangedEvent.OnEventRaised -= HandleCharacterLevelChanged;
            stageChangedEvent.OnEventRaised -= HandleCharacterStageChanged;
        }

        // --------------------------------------------------
        // Party Slot Selection
        // --------------------------------------------------

        private void HandlePartySlotClicked(int slotIndex)
        {
            if (!_partyManager.IsSlotUnlocked(slotIndex))
                return;

            // Clicking the current selected slot again simply cancels the selection.
            if (_selectedSlotIndex == slotIndex)
            {
                _selectedSlotIndex = -1;
                _assignmentMode = false;

                RefreshAll();
                return;
            }

            // Select a new slot, but do NOT enter assignment mode yet.
            _selectedSlotIndex = slotIndex;
            _assignmentMode = false;

            RefreshAll();
        }
        
        private void HandleSlotActionClicked(int slotIndex)
        {
            if (_selectedSlotIndex != slotIndex)
                return;

            if (!_partyManager.IsSlotUnlocked(slotIndex))
                return;

            _assignmentMode = true;

            RefreshAll();
        }

        // --------------------------------------------------
        // Roster Interaction
        // --------------------------------------------------

        private void HandleRosterCharacterClicked(CharacterDefinition definition)
        {
            if (!_partyManager.IsCharacterUnlocked(definition))
            {
                OpenPurchasePopup(definition);
                return;
            }

            if (!_assignmentMode || _selectedSlotIndex < 0)
                return;

            PartyChangeResult result = _partyManager.TryAssignCharacterToSlot(definition, _selectedSlotIndex);

            switch (result)
            {
                case PartyChangeResult.Success:
                    _selectedSlotIndex = -1;
                    _assignmentMode = false;
                    
                    RefreshAll();
                    break;

                case PartyChangeResult.CharacterResting:
                case PartyChangeResult.CharacterAlreadyAssigned:
                case PartyChangeResult.SlotLocked:
                    RefreshAll();
                    break;

                default:
                    Debug.LogWarning($"[PartyPanelController] Party assignment failed: {result}.", this);
                    break;
            }
        }

        // --------------------------------------------------
        // Purchase
        // --------------------------------------------------

        private void OpenPurchasePopup(CharacterDefinition definition)
        {
            Sprite portrait = CharacterPortraitResolver.GetPortrait(definition, _saveManager);

            purchasePopup.Show(definition, portrait);
        }

        private void HandlePurchaseRequested(CharacterDefinition definition)
        {
            PartyChangeResult result = _partyManager.TryPurchaseCharacter(definition);

            switch (result)
            {
                case PartyChangeResult.Success:
                    purchasePopup.Hide();

                    RefreshAll();
                    break;

                case PartyChangeResult.InsufficientGold:
                    purchasePopup.ShowInsufficientGold();
                    break;

                case PartyChangeResult.CharacterAlreadyUnlocked:
                    purchasePopup.Hide();

                    RefreshAll();
                    break;

                default:
                    Debug.LogWarning($"[PartyPanelController] Character purchase failed: {result}.", this);
                    break;
            }
        }

        private void HandleGoldChanged(int gold)
        {
            // Gold can increase while this menu is open because gameplay continues in the background.
            // Clear stale "not enough gold" feedback so the player can simply try again.
            if (purchasePopup.IsOpen)
                purchasePopup.ClearFeedback();
        }
        
        // --------------------------------------------------
        // Character Evolution and Level Up
        // --------------------------------------------------
        
        private void HandleCharacterLevelChanged(CharacterLevelChangedPayload payload)
        {
            if (!_initialized)
                return;

            RefreshPartySlots();
        }
        
        private void HandleCharacterStageChanged(CharacterStageChangedPayload payload)
        {
            if (!_initialized)
                return;

            RefreshPartySlots();
            RefreshRosterPresentation();
        }

        // --------------------------------------------------
        // Domain Events
        // --------------------------------------------------

        private void HandlePartyChanged()
        {
            RefreshAll();
        }

        private void HandleCharacterUnlocked(string characterId)
        {
            RefreshAll();
        }

        private void HandlePartySlotUnlocked(int slotIndex)
        {
            RefreshAll();
        }

        // --------------------------------------------------
        // Refresh
        // --------------------------------------------------

        private void RefreshAll()
        {
            if (!_initialized)
                return;

            RefreshPartySlots();
            RefreshRosterPresentation();
            RefreshRosterRuntimeState();
        }

        private void RefreshPartySlots()
        {
            for (int slotIndex = 0; slotIndex < partySlots.Length; slotIndex++)
            {
                PartySlotView view = partySlots[slotIndex];

                if (view == null)
                    continue;

                bool unlocked = _partyManager.IsSlotUnlocked(slotIndex);

                CharacterDefinition character = _partyManager.GetCharacterAt(slotIndex);

                bool occupied = character != null;

                Sprite portrait = null;
                string characterName = string.Empty;
                int level = 0;
                float xpProgress = 0f;
                
                if (occupied)
                {
                    portrait = CharacterPortraitResolver.GetPortrait(character, _saveManager);

                    characterName = character.baseStats != null ? character.baseStats.displayName : string.Empty;

                    if (character.baseStats != null && character.prefab != null && character.prefab.TryGetComponent(out CharacterLevelSubsystem levelSubsystem) && levelSubsystem.LevelUpData != null)
                    {
                        string characterId = character.baseStats.characterId;
                        int xp = _saveManager.GetCharacterXp(characterId);
                        level = levelSubsystem.LevelUpData.GetLevelForXp(xp);
                        xpProgress = levelSubsystem.LevelUpData.GetLevelProgress(xp);
                    }
                }
               
                bool selected = _selectedSlotIndex == slotIndex;
                string actionLabel = null;
                if (selected)
                {
                    actionLabel = occupied ? "Switch?" : "Send?";
                }
                
                view.SetState(unlocked, occupied, portrait, characterName, level, xpProgress, _selectedSlotIndex == slotIndex, actionLabel);
            }
        }

        private void RefreshRosterPresentation()
        {
            foreach (CharacterRosterItemView item in characterItems)
            {
                if (item == null)
                    continue;

                CharacterDefinition definition = item.CharacterDefinition;

                if (definition == null)
                {
                    Debug.LogError("[PartyPanelController] Character roster item has no CharacterDefinition.", item);
                    continue;
                }

                bool unlocked = _partyManager.IsCharacterUnlocked(definition);

                Sprite portrait = CharacterPortraitResolver.GetPortrait(definition, _saveManager);

                item.SetPresentation(portrait, unlocked);
            }
        }

        private void RefreshRosterRuntimeState()
        {
            foreach (CharacterRosterItemView item in characterItems)
            {
                if (item == null || item.CharacterDefinition == null)
                    continue;

                CharacterDefinition definition = item.CharacterDefinition;

                bool unlocked = _partyManager.IsCharacterUnlocked(definition);

                int assignedSlot = _partyManager.FindAssignedSlot(definition);

                bool inBattle = assignedSlot >= 0;

                float restRemaining = unlocked ? _partyManager.GetCharacterRestRemaining(definition) : 0f;

                bool canAssign = _assignmentMode &&
                                 _selectedSlotIndex >= 0 &&
                                 unlocked &&
                                 !inBattle &&
                                 restRemaining <= 0f;

                item.SetRuntimeState(unlocked, inBattle, restRemaining, canAssign);
            }
        }
    }
}