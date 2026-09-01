using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.GameMenu
{
    public sealed class SimpleButtonDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button button;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        
        [Header("Selected State")]
        [SerializeField] private Material normalMaterial;
        [SerializeField] private Sprite normalBg;

        [Header("Unselected State")]
        [SerializeField] private Material grayscaleMaterial;
        [SerializeField] private Sprite darkBg;

        private bool _selected;

        private void Awake()
        {
            button.onClick.AddListener(Toggle);
            ApplyVisualState();
        }

        private void OnDestroy()
        {
            button.onClick.RemoveListener(Toggle);
        }

        private void Toggle()
        {
            _selected = !_selected;
            ApplyVisualState();
        }

        private void ApplyVisualState()
        {
            iconImage.material =
                _selected
                    ? normalMaterial
                    : grayscaleMaterial;

            backgroundImage.sprite =
                _selected
                    ? normalBg
                    : darkBg;
        }
    }
}
