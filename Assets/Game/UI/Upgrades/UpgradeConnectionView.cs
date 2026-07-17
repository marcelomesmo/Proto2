using System;
using Game.Enum;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Upgrades
{
    public sealed class UpgradeConnectionView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform rect;
        [SerializeField] private Image image;

        [Header("Visuals")] 
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;
        [SerializeField] private Color inactiveColor = new Color32(105, 73, 43, 255); // brown
        [SerializeField] private Color availableColor = new Color32(90, 190, 255, 255); // blue
        [SerializeField] private Color purchasedColor = new Color32(90, 190, 255, 255); // blue

        private void Awake()
        {
            ResolveReferences();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ResolveReferences();
        }
#endif
        
        private void ResolveReferences()
        {
            if (rect == null)
                rect = transform as RectTransform;

            if (image == null)
                image = GetComponentInChildren<Image>();
        }
        
        public void SetLength(float length)
        {
            if (rect == null)
                ResolveReferences();

            rect.pivot = new Vector2(0f, 0.5f);

            rect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                length);
        }

        public void SetState(UpgradeConnectionState state)
        {
            if (image == null)
                ResolveReferences();

            if (image == null)
                return;

            switch (state)
            {
                case UpgradeConnectionState.Inactive:
                    ApplyVisual(inactiveColor, inactiveSprite);
                    break;

                case UpgradeConnectionState.Available:
                    ApplyVisual(availableColor, inactiveSprite);
                    break;
                
                case UpgradeConnectionState.Purchased:
                    ApplyVisual(purchasedColor, activeSprite);
                    break;
            }
        }

        private void ApplyVisual(
            Color color, Sprite sprite) // Can instead do a struct ConnectionVisual{ Sprite, Color} later, if we grow this
        {
            if(sprite != null)
                image.sprite = sprite;
    
            image.color = color;
        }
    }
}