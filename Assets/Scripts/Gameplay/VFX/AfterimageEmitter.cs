using Services.Manager;
using UnityEngine;

namespace Gameplay.VFX
{
    public class AfterimageEmitter : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private GameObject afterimagePrefab;
        [SerializeField] private SpriteRenderer sourceRenderer;
        [SerializeField] private float spawnInterval = 0.06f;
    
        private float _timer;
        private bool _active;
        
        public void SetActive(bool value)
        {
            _active = value;
            _timer = 0f;
        }
        
        private void Update()
        {
            if (!_active)
                return;
            
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                Emit();
                _timer = spawnInterval;
            }
        }

        private void Emit()
        {
            if (sourceRenderer.sprite == null)
                return;
            
            var obj = VFXPoolManager.Instance.Spawn(afterimagePrefab);
            var afterimage = obj.GetComponent<AfterimageVFX>();

            afterimage.Initialize(
                sourceRenderer.sprite,
                transform.position,
                sourceRenderer.flipX,
                sourceRenderer.flipY,
                sourceRenderer,
                afterimagePrefab
            );
        }
    }
}