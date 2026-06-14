using Core.Enum;
using TMPro;
using UnityEngine;

namespace Core.VFX
{
    public class FloatingDamageVFX : BaseVFX
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

        private Color _originalColor;

        private void Awake()
        {
            _originalColor = floatingText.color;
        }

        public void Initialize(CombatAction actionType, int amount, GameObject prefab)
        {
            PrefabReference = prefab;
            Timer = lifetime;

            floatingText.text = amount.ToString();
            switch (actionType)
            {
                case CombatAction.Heal:
                    floatingText.color = healingColor;
                    break;
                case CombatAction.Damage:
                    floatingText.color = damageColor;
                    break;
                default:
                    floatingText.color = _originalColor;
                    break;
            }

            Initialized = true;
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
