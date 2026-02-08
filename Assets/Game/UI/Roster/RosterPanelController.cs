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
        [Header("Data")]
        [SerializeField] private List<CharacterDefinition> allCharacters;
        [SerializeField] private PlayerLoadoutData loadout;

        [Header("UI")]
        [SerializeField] private Transform contentRoot;
        [SerializeField] private RosterCharacterButton buttonPrefab;
        [SerializeField] private CharacterTooltip tooltip;

        private readonly List<RosterCharacterButton> _buttons = new();

        private void Awake()
        {
            Build();
        }

        private void Build()
        {
            Clear();
            
            foreach (var character in allCharacters)
            {
                var btn = Instantiate(buttonPrefab, contentRoot);
                btn.Initialize(character, loadout, this);
                _buttons.Add(btn);
            }
        }

        public void Refresh()
        {
            foreach (var btn in _buttons)
                btn.Refresh();
        }

        private void Clear()
        {
            foreach (var btn in _buttons)
                Destroy(btn.gameObject);

            _buttons.Clear();
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
