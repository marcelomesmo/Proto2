using System;
using System.Collections.Generic;
using Game.Entity.Player;
using Game.Entity.Player.Progression;
using UnityEngine;

namespace Game.UI.Roster
{
    public sealed class InitialCharacterSelectionPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        [SerializeField] private List<InitialCharacterSelectionButton> buttons;
        
        [Header("UI")]
        [SerializeField] private InitialCharacterTooltip tooltip;

        public event Action<CharacterDefinition> CharacterSelected;

        private void Awake()
        {
            Build();
        }
        
        private void Build()
        {
            foreach (InitialCharacterSelectionButton button in buttons)
            {
                if (button == null)
                    continue;

                button.Initialize(this);
                
                button.Clicked += HandleButtonClicked;
            }
        }

        private void OnDestroy()
        {
            foreach (InitialCharacterSelectionButton button in buttons)
            {
                if (button == null)
                    continue;

                button.Clicked -= HandleButtonClicked;
            }
        }

        public void Show()
        {
            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);
        }

        private void HandleButtonClicked(CharacterDefinition definition)
        {
            CharacterSelected?.Invoke(definition);
        }
        
        #region Tooltip

        public void ShowTooltip(CharacterEvolutionData character)
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