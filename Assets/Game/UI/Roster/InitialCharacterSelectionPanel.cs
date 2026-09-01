using System;
using System.Collections.Generic;
using Game.Entity.Player;
using Game.Entity.Player.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Roster
{
    public sealed class InitialCharacterSelectionPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        [SerializeField] private List<InitialCharacterSelectionButton> buttons;
        
        [Header("UI")]
        [SerializeField] private InitialCharacterTooltip tooltip;

        [Header("Start Game")]
        [SerializeField] private Button startButton;
        
        private CharacterDefinition _selectedCharacter;

        public event Action<CharacterDefinition> CharacterSelected;
        public event Action<CharacterDefinition> GameStarted;

        private void Awake()
        {
            Build();
            
            startButton.onClick.AddListener(HandleStartButtonClicked);
            startButton.interactable = false;
        }
        
        private void Build()
        {
            foreach (InitialCharacterSelectionButton button in buttons)
            {
                if (button == null)
                    continue;

                button.Initialize(this);
                
                button.Clicked += HandleCharacterButtonClicked;
            }
        }

        private void OnDestroy()
        {
            foreach (InitialCharacterSelectionButton button in buttons)
            {
                if (button == null)
                    continue;

                button.Clicked -= HandleCharacterButtonClicked;
            }
            
            startButton.onClick.RemoveListener(HandleStartButtonClicked);
        }

        public void Show()
        {
            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);
        }

        private void HandleCharacterButtonClicked(CharacterDefinition definition)
        {
            _selectedCharacter = definition;
            
            // Disable/enable highlight and selection for buttons
            foreach (InitialCharacterSelectionButton button in buttons)
            {
                if (button == null)
                    continue;

                button.SetSelectedVisual(button.CharacterDefinition == _selectedCharacter);
            }

            startButton.interactable = _selectedCharacter != null;

            CharacterSelected?.Invoke(_selectedCharacter);
        }
        
        private void HandleStartButtonClicked()
        {
            if (_selectedCharacter == null)
                return;

            GameStarted?.Invoke(_selectedCharacter);
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