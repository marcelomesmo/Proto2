using System.Collections.Generic;
using Game.Entity.Player;
using Game.Entity.Player.Progression;
using UnityEngine;

namespace Game.UI.Roster
{
    /*
        The panel:
        - Knows all characters
        - Builds buttons once
        - Refreshes buttons on state change
     */
    public class RosterPanelController : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private List<RosterCharacterButton> characterButtons;
        [SerializeField] private PlayerLoadoutData loadout;

        [Header("UI")]
        [SerializeField] private CharacterTooltip tooltip;
        
        [Header("Party Preview")]
        [SerializeField] private PartyPreviewController partyPreview;
        [Header("World Selector")]
        [SerializeField] private LevelSelectorController levelSelector;

        public bool HasCharacterSelected => loadout.CurrentPartySize > 0;
        public string GetLevelSelectedSceneName => levelSelector.SelectedSceneName;
        public bool IsCurrentLevelLocked => levelSelector.IsCurrentLevelLocked;

        private void Awake()
        {
            Build();
        }

        private void Build()
        {
            Clear();

            foreach (var btn in characterButtons)
            {
                btn.Initialize(loadout, this);
            }
        }
        
        private void Start()
        {
            Refresh();
        }

        public void Refresh()
        {
            foreach (var btn in characterButtons)
                btn.Refresh();
            
            partyPreview?.Refresh();
        }

        private void Clear()
        {
            HideTooltip();
        }

        #region Tooltip

        public void ShowTooltip(CharacterDefinition character, CharacterEvolutionData evolutionData)
        {
            if (tooltip == null)
                return;

            tooltip.Show(character, evolutionData);
        }

        public void HideTooltip()
        {
            tooltip?.Hide();
        }

        #endregion
    }
}
