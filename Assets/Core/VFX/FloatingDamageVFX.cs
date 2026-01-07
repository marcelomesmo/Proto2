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
        [SerializeField] private TMP_Text damageText;

        private Color _originalColor;

        private void Awake()
        {
            _originalColor = damageText.color;
        }

        public void Initialize(int amount, GameObject prefab)
        {
            PrefabReference = prefab;
            Timer = lifetime;

            damageText.text = amount.ToString();
            damageText.color = _originalColor;

            Initialized = true;
        }

        protected override void OnUpdate()
        {
            transform.position += floatVelocity * Time.deltaTime;

            float t = 1f - (Timer / lifetime);
        
            Color c = damageText.color;
            c.a = alphaOverLifetime.Evaluate(t);
            damageText.color = c;
        }
    }
}
