using System.Collections.Generic;
using Core.Gameplay.Combat.Attack;
using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Combat.AreaEffect
{
    public class AreaEffectInstance : MonoBehaviour
    {
        private AreaEffectData _data;
        private DamagePayload _payload;
        private float _remainingTime;
        private float _tickTimer;
        
        private readonly List<Collider2D> _hits = new();
        
        private GameObject _vfxInstance;

        public void Initialize(
            AreaEffectData data,
            DamagePayload payload)
        {
            _data = data;
            _payload = payload;

            _remainingTime = data.duration;
            _tickTimer = Mathf.Max(0.01f, data.tickInterval);
            
            if (_data.vfxPrefab)
            {
                _vfxInstance = Instantiate(
                    _data.vfxPrefab,
                    transform.position,
                    Quaternion.identity,
                    _data.followOwner ? transform : null
                );
            }
        }

        private void Update()
        {
            _remainingTime -= Time.deltaTime;
            _tickTimer -= Time.deltaTime;

            if (_remainingTime <= 0f)
            {
                if (_vfxInstance)
                    Destroy(_vfxInstance);
                
                Destroy(gameObject); // pooled later
                return;
            }

            if (_tickTimer <= 0f)
            {
                ApplyTick();
                _tickTimer = _data.tickInterval;
            }
        }

        private void ApplyTick()
        {
            _hits.Clear();

            var filter = new ContactFilter2D
            {
                useTriggers = true,
                layerMask = _data.hitLayers
            };
            
            if (_data.shape == AreaShape.Circle)
            {
                Physics2D.OverlapCircle(
                    transform.position,
                    _data.radius,
                    filter,
                    _hits);
            }
            else if (_data.shape == AreaShape.Box)
            {
                Physics2D.OverlapBox(
                    transform.position,
                    _data.boxSize,
                    0f,
                    filter,
                    _hits);
            }

            foreach (var hit in _hits)
            {
                if (!hit.TryGetComponent<IDamageable>(out var damageable))
                    continue;

                if (!damageable.CanBeDamaged())
                    continue;

                damageable.TakeDamage(_payload);
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_data == null)
                return;

            Gizmos.color = Color.cyan;
            
            switch (_data.shape)
            {
                case AreaShape.Circle:
                    Gizmos.DrawWireSphere(transform.position, _data.radius);
                    break;

                case AreaShape.Box:
                    Gizmos.DrawWireCube(transform.position, _data.boxSize);
                    break;
                
                case AreaShape.Cone:
                    Gizmos.DrawWireCube(transform.position, _data.boxSize);
                    break;
            }
            
            float t = Mathf.Clamp01(_remainingTime / _data.duration);
            Gizmos.color = Color.Lerp(Color.red, Color.green, t);
            
            switch (_data.shape)
            {
                case AreaShape.Circle:
                    Gizmos.DrawSphere(transform.position, _data.radius);
                    break;

                case AreaShape.Box:
                    Gizmos.DrawCube(transform.position, _data.boxSize);
                    break;
                
                case AreaShape.Cone:
                    Gizmos.DrawCube(transform.position, _data.boxSize);
                    break;
            }
        }
#endif
    }
}
