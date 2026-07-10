using Core.Enum;
using Core.Gameplay.Combat.Attack;
using TMPro;
using UnityEngine;

namespace Core.VFX
{
    public class FloatingDamageVFX : PooledTimedVFX
    {
        [Header("Motion")]
        [SerializeField] private Vector3 floatVelocity = new(0f, 1.5f, 0f);

        [Header("Fade")]
        [SerializeField] private AnimationCurve alphaOverLifetime;

        [Header("Text")]
        [SerializeField] private TMP_Text floatingText;
        
        [Header("Visuals")]
        [SerializeField] private Color damageColor;
        [SerializeField] private Color healingColor;
        [SerializeField] private Color resistColor;
        [SerializeField] private float critFontSize;

        private Color _originalColor;
        private float _originalFontSize;

        private void Awake()
        {
            _originalColor = floatingText.color;
            _originalFontSize = floatingText.fontSize;
        }

        public void Initialize(CombatPayload payload)
        {
            floatingText.text = payload.amount.ToString();
            floatingText.fontSize = _originalFontSize;
            
            floatingText.color = payload.action switch
            {
                CombatAction.Heal   => healingColor,
                CombatAction.Damage => damageColor,
                _                   => _originalColor
            };

            if (payload.IsCritical)
                floatingText.fontSize = critFontSize;

            if (payload.HasResisted)
            {
                floatingText.fontSize = _originalFontSize / 2;  // TODO: add better feedback later
                floatingText.color = resistColor;
            }
        }

        protected override void OnUpdate()
        {
            transform.position += floatVelocity * Time.deltaTime;

            float t = 1f - (Timer / lifetime);
            Color c = floatingText.color;
            c.a = alphaOverLifetime.Evaluate(t);
            floatingText.color = c;
        }
    }
}
