using Core.VFX;
using Game.Enum;
using Game.Loot;
using Game.UI;
using TMPro;
using UnityEngine;

namespace Game.VFX
{
    public class FloatingLootVFX : PooledTimedVFX
    {
        [Header("Motion")]
        [SerializeField] private Vector3 floatVelocity = new(0f, 1.5f, 0f);

        [Header("Fade")]
        [SerializeField] private AnimationCurve alphaOverLifetime =
            AnimationCurve.Linear(0f, 1f, 1f, 0f);

        [Header("Text")]
        [SerializeField] private TMP_Text floatingText;
        
        [Header("Visuals")]
        [SerializeField] private Color goldLootColor;

        private Color _originalColor;
        private float _originalFontSize;

        private bool _initialized;

        private void Awake()
        {
            if (!floatingText)
            {
                Debug.LogError($"{name}: FloatingLootVFX is missing TMP_Text.", this);
                return;
            }
            
            _originalColor = floatingText.color;
            _originalFontSize = floatingText.fontSize;
        }

        private void OnEnable()
        {
            _initialized = false;

            if (!floatingText)
                return;
            
            floatingText.color = _originalColor;
            floatingText.fontSize = _originalFontSize;
        }

        public void Initialize(GameLoot loot)
        {
            if (!floatingText)
            {
                Debug.LogError($"{name}: FloatingLootVFX is missing TMP_Text.", this);
                return;
            }
            
            string iconText = string.Empty;
            Color textColor = _originalColor;

            // Later we can do a LootTypeIconRegistrySO, but this is good enough for now.
            switch (loot.LootType)
            {
                case LootType.Gold:
                    iconText = TMPIcons.Currency;
                    textColor = goldLootColor;
                    break;
                case LootType.Unknown:
                    break;
            }
            
            floatingText.text = $"{iconText}{loot.LootValue}";
            floatingText.color = textColor;
            
            _initialized = true;
        }

        protected override void OnUpdate()
        {
            if (!_initialized)
                return;
            
            transform.position += floatVelocity * Time.deltaTime;

            float t = 1f - (Timer / lifetime);
            Color c = floatingText.color;
            c.a = alphaOverLifetime.Evaluate(t);
            floatingText.color = c;
        }
    }
}