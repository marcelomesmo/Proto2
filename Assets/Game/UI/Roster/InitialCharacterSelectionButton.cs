using System;
using Game.Entity.Player;
using Game.Entity.Player.Progression;
using TMPro;
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
        
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI characterName;
        [SerializeField] private Color normalNameColor = Color.gray;
        [SerializeField] private Color highlightedNameColor = Color.white;

        public CharacterDefinition CharacterDefinition => characterDefinition;

        public event Action<CharacterDefinition> Clicked;
        
        private InitialCharacterSelectionPanel _panel;
        
        private bool _selected;

        public void Initialize(InitialCharacterSelectionPanel panel)
        {
            _panel = panel;
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClicked);
            
            if (characterDefinition?.baseStats != null)
            {
                characterName.text = characterDefinition.baseStats.displayName;
            }

            characterName.color = normalNameColor;

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
        
        public void SetSelectedVisual(bool selected)
        {
            _selected = selected;
            
            characterName.color = _selected ? highlightedNameColor : normalNameColor;
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            characterName.color = highlightedNameColor;
            _panel.ShowTooltip(characterEvolutionData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(!_selected)
                characterName.color = normalNameColor;
            _panel.HideTooltip();
        }
    }
}