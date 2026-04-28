using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.CharacterHUD
{
    public sealed class CharacterHUDSlotView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image portraitImage;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Config")]
        [SerializeField] private CharacterPortraitResolver portraitResolver;

        public void SetLevel(int level)
        {
            levelText.text = level.ToString();

            if (level == 0)
                levelText.text = "";
        }

        public void SetStage(int stage)
        {
            portraitResolver.SetStage(stage);
            RefreshPortrait();
        }

        public void SetPortrait(Sprite portrait)
        {
            if(portrait)
                portraitImage.sprite = portrait;
        }

        private void RefreshPortrait()
        {
            portraitImage.sprite = portraitResolver.Resolve();
        }
    }
}