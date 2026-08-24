using System;
using Game.Entity.Player;
using Game.Entity.Player.Progression;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Roster
{
    public sealed class InitialCharacterSelectionButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField] private Button button;

        [SerializeField] private CharacterDefinition characterDefinition;
        [SerializeField] private CharacterEvolutionData characterEvolutionData;

        public CharacterDefinition CharacterDefinition => characterDefinition;

        public event Action<CharacterDefinition> Clicked;
        
        private InitialCharacterSelectionPanel _panel;

        public void Initialize(InitialCharacterSelectionPanel panel)
        {
            _panel = panel;
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClicked);
        }

        private void HandleClicked()
        {
            if (characterDefinition == null)
            {
                Debug.LogError("[InitialCharacterSelectionButton] Missing CharacterDefinition.", this);
                return;
            }

            Clicked?.Invoke(characterDefinition);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            _panel.ShowTooltip(characterEvolutionData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _panel.HideTooltip();
        }
    }
}