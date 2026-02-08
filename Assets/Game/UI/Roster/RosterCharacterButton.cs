using Core.Services;
using Core.Services.Save;
using Game.Entity.Player;
using Game.Services.Save;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Roster
{
    public class RosterCharacterButton : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("UI")] 
        [SerializeField] private Button button;
        [SerializeField] private Image characterImage;
        [SerializeField] private TextMeshProUGUI characterName;
        
        [Header("Visuals")]
        [SerializeField] private Image selectedOverlay;
        [SerializeField] private Sprite lockedPortrait;
        [SerializeField] private Material grayscaleMaterial;
        [SerializeField] private Material normalMaterial;
        
        private CharacterDefinition _character;
        private PlayerLoadoutData _loadout;
        private RosterPanelController _panel;
        private GameSaveManager _saveManager;

        public void Initialize(
            CharacterDefinition character,
            PlayerLoadoutData loadout,
            RosterPanelController panel
        )
        {
            _character = character;
            _loadout = loadout;
            _panel = panel;
            _saveManager = ServiceLocator.Get<ISaveManager>() as GameSaveManager;
            if (_saveManager == null)
            {
                Debug.LogError("[RosterCharacterButton] No SaveManager registered.");
                return;
            }
            
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClicked);

            Refresh();
        }

        public void Refresh()
        {
            bool isUnlocked = _saveManager.IsCharacterUnlocked(_character.baseStats.characterId);
            bool isSelected = _loadout.Contains(_character);
            
            if (!isUnlocked)
            {
                characterName.text = "???";
                characterImage.sprite = lockedPortrait;
                characterImage.material = normalMaterial;
                button.interactable = false;
                return;
            }
            
            characterName.text = _character.name;
            characterImage.sprite = _character.rosterScreenPortrait;

            characterImage.material = isSelected ? normalMaterial : grayscaleMaterial;
            
            if (selectedOverlay != null)
                selectedOverlay.enabled = isSelected;
            
            button.interactable = true;
        }
        
        private void OnClicked()
        {
            bool isUnlocked = _saveManager.IsCharacterUnlocked(_character.baseStats.characterId);
            if (!isUnlocked)
                return;
            
            if (_loadout.Contains(_character))
                _loadout.RemoveFromParty(_character);
            else
            {
                if (!_loadout.AddToParty(_character))
                    return; // party full, or invalid
            }
    
            _panel.Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _panel.ShowTooltip(_character);
            
            // this doesnt work as the transform is the same for all the buttons,
            // they change positions through the grid but transform is virtually the same.
            //_panel.ShowTooltip(_character, transform as RectTransform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _panel.HideTooltip();
        }
    }
}
