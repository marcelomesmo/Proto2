using UnityEngine;

namespace Gameplay.VFX
{
    public class HitFlash : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private float flashDuration = 0.1f;
    
        private Material _mat;
        private float _flashTimer = 0f;
        private bool _flashing = false;

        private void Start()
        {
            if (targetRenderer == null)
                targetRenderer = GetComponentInChildren<Renderer>();

            _mat = targetRenderer.material;
            _mat.SetFloat("_FlashAmount", 0f);
        }

        private void Update()
        {
            if (!_flashing)
                return;

            _flashTimer -= Time.deltaTime;

            if (_flashTimer <= 0f)
            {
                _flashing = false;
                _mat.SetFloat("_FlashAmount", 0f);
            }
        }
    
        public void Flash()
        {
            _flashing = true;
            _flashTimer = flashDuration;
            _mat.SetFloat("_FlashAmount", 1f);
        }
    }
}