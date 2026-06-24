using Core.Services.Manager;
using Core.VFX;
using UnityEngine;

namespace Core.Gameplay.Entity.VFX
{
    public class AfterimageEmitter : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private PooledVFX  afterimagePrefab;
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
            
            var instance = VFXPoolManager.Instance.Spawn(
                afterimagePrefab,
                transform.position,
                Quaternion.identity);
            
            if (instance is AfterimageVFX afterimage)
            {
                afterimage.Initialize(
                    sourceRenderer.sprite,
                    transform.position,
                    sourceRenderer.flipX,
                    sourceRenderer.flipY,
                    sourceRenderer);
            }
        }
    }
}