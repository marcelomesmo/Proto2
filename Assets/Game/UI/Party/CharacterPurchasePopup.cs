using System;
using Game.Entity.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Party
{
    public sealed class CharacterPurchasePopup : MonoBehaviour
    {
        [SerializeField] private GameObject root;

        [Header("Character")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI costText;

        [Header("Feedback")]
        [SerializeField] private GameObject insufficientGoldRoot;

        [Header("Buttons")]
        [SerializeField] private Button buyButton;
        [SerializeField] private Button cancelButton;

        private CharacterDefinition _character;

        public bool IsOpen => root.activeSelf;

        public event Action<CharacterDefinition> BuyClicked;

        private void Awake()
        {
            buyButton.onClick.AddListener(HandleBuyClicked);

            cancelButton.onClick.AddListener(Hide);

            Hide();
        }

        private void OnDestroy()
        {
            buyButton.onClick.RemoveListener(HandleBuyClicked);

            cancelButton.onClick.RemoveListener(Hide);
        }

        public void Show(CharacterDefinition character, Sprite portrait)
        {
            _character = character;

            portraitImage.sprite = portrait;

            costText.text = character.unlockCost.ToString();

            insufficientGoldRoot.SetActive(false);

            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);

            _character = null;
        }

        public void ShowInsufficientGold()
        {
            insufficientGoldRoot.SetActive(true);
        }

        public void ClearFeedback()
        {
            insufficientGoldRoot.SetActive(false);
        }

        private void HandleBuyClicked()
        {
            if (_character == null)
                return;

            BuyClicked?.Invoke(_character);
        }
    }
}