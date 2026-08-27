using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Party
{
    public sealed class PartySlotView : MonoBehaviour
    {
        [SerializeField] private Button button;

        [Header("States")]
        [SerializeField] private GameObject lockedRoot;
        [SerializeField] private GameObject emptyRoot;
        [SerializeField] private GameObject occupiedRoot;

        [Header("Occupied")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image xpBarFill;

        [Header("Selection")]
        [SerializeField] private GameObject selectedHighlight;
        
        [Header("Slot Action")]
        [SerializeField] private GameObject slotActionRoot;
        [SerializeField] private Button slotActionButton;
        [SerializeField] private TextMeshProUGUI slotActionText;

        private int _slotIndex;

        public event Action<int> Clicked;
        public event Action<int> ActionClicked;

        private void Awake()
        {
            button.onClick.AddListener(HandleClicked);
            slotActionButton.onClick.AddListener(HandleActionClicked);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleClicked);
            slotActionButton.onClick.RemoveListener(HandleActionClicked);
        }

        public void Initialize(int slotIndex)
        {
            _slotIndex = slotIndex;
        }

        public void SetState(bool unlocked, bool occupied, Sprite portrait, string characterName, int level, float xpProgress, bool selected, string actionLabel)
        {
            lockedRoot.SetActive(!unlocked);
            emptyRoot.SetActive(unlocked && !occupied);
            occupiedRoot.SetActive(unlocked && occupied);

            if (portraitImage != null)
            {
                portraitImage.sprite = portrait;
                portraitImage.gameObject.SetActive(unlocked && occupied && portrait != null);   // Hide when not assigned
            }

            if (occupied)
            {
                characterNameText.text = characterName;
                levelText.text = $"Lv. {level}";
                xpBarFill.fillAmount = Mathf.Clamp01(xpProgress);
            }
            
            selectedHighlight.SetActive(unlocked && selected);
            
            // Retreat Button
            bool showAction = unlocked && selected && !string.IsNullOrEmpty(actionLabel);

            slotActionRoot.SetActive(showAction);

            if (showAction)
                slotActionText.text = actionLabel;
            
            button.interactable = unlocked;
        }

        private void HandleClicked()
        {
            Clicked?.Invoke(_slotIndex);
        }
        
        private void HandleActionClicked()
        {
            ActionClicked?.Invoke(_slotIndex);
        }
    }
}