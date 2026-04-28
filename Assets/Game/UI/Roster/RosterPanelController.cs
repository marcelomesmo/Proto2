using System.Collections.Generic;
using Game.Entity.Player;
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

        public void Refresh()
        {
            foreach (var btn in characterButtons)
                btn.Refresh();
        }

        private void Clear()
        {
            HideTooltip();
        }

        #region Tooltip

        public void ShowTooltip(CharacterDefinition character)
        {
            if (tooltip == null)
                return;

            tooltip.Show(character);
        }

        public void HideTooltip()
        {
            tooltip?.Hide();
        }

        #endregion
    }
}
