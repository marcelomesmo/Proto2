using System;
using Game.Entity.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Party
{
    public sealed class CharacterRosterItemView : MonoBehaviour
    {
        [Header("Character")]
        [SerializeField] private CharacterDefinition characterDefinition;

        [Header("Interaction")]
        [SerializeField] private Button button;

        [Header("Presentation")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private Material normalPortraitMaterial;
        [SerializeField] private Material grayscalePortraitMaterial;

        [Header("States")]
        [SerializeField] private GameObject lockedOverlay;
        [SerializeField] private GameObject inBattleIndicator;
        [SerializeField] private GameObject candidateHighlight;
        [SerializeField] private GameObject restingRoot;
        [SerializeField] private TextMeshProUGUI restingText;

        public CharacterDefinition CharacterDefinition => characterDefinition;

        public event Action<CharacterDefinition> Clicked;

        private void Awake()
        {
            button.onClick.AddListener(HandleClicked);
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(HandleClicked);
        }

        public void SetPresentation(Sprite portrait, bool unlocked)
        {
            portraitImage.sprite = portrait;

            portraitImage.material = unlocked ? normalPortraitMaterial : grayscalePortraitMaterial;

            lockedOverlay.SetActive(!unlocked);
        }

        public void SetRuntimeState(bool unlocked, bool inBattle, float restRemaining, bool canAssign)
        {
            bool resting = restRemaining > 0f;
            
            bool grayscale = !unlocked || resting;
            portraitImage.material = grayscale ? grayscalePortraitMaterial : normalPortraitMaterial;

            inBattleIndicator.SetActive(unlocked && inBattle);

            restingRoot.SetActive(unlocked && resting);

            if (resting && restingText != null)
            {
                restingText.text = $"{Mathf.CeilToInt(restRemaining)}s";
            }

            candidateHighlight.SetActive(unlocked && canAssign);
            
            // Locked characters remain clickable because clicking them opens the purchase popup.
            // Unlocked characters are clickable only if they are currently valid candidates for the selected party slot.
            button.interactable = !unlocked || canAssign;
        }

        private void HandleClicked()
        {
            if (characterDefinition == null)
            {
                Debug.LogError("[CharacterRosterItemView] Missing CharacterDefinition.", this);
                return;
            }

            Clicked?.Invoke(characterDefinition);
        }
    }
}