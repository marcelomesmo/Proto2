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
            
            // todo: also don't have the vfx for the effect instantiated as we dont instantiate a prefab
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

            var filter = _payload.source.targetFilter.ToContactFilter();
            
            switch (_data.shape)
            {
                case AreaShape.Circle:
                    Physics2D.OverlapCircle(
                        transform.position,
                        _data.circleRadius,
                        filter,
                        _hits);
                    break;
                
                case AreaShape.Box:
                    Physics2D.OverlapBox(
                        transform.position,
                        _data.boxSize,
                        0f,
                        filter,
                        _hits);
                    break;
                
                case AreaShape.Cone:
                    Physics2D.OverlapCircle(
                        transform.position,
                        _data.coneRadius,
                        filter,
                        _hits
                    );
                    break;
            }
            
            // For cone only
            Vector2 origin = transform.position;
            Vector2 forward = transform.right;
            float halfAngle = _data.coneAngle * 0.5f;

            foreach (var hit in _hits)
            {
                if (!_payload.source.targetFilter.CanHit(hit))
                    continue;
                
                if (!hit.TryGetComponent<IDamageable>(out var damageable))
                    continue;

                if (!damageable.CanBeDamaged())
                    continue;
                
                // For cone only
                if (_data.shape == AreaShape.Cone)
                {
                    if (!IsInsideCone(
                            origin,
                            forward,
                            hit.bounds.center,
                            _data.coneRadius,
                            halfAngle))
                        continue;
                }

                damageable.TakeDamage(_payload);
            }
        }
        
        private bool IsInsideCone(
            Vector2 origin,
            Vector2 forward,
            Vector2 targetPos,
            float radius,
            float halfAngleDeg)
        {
            Vector2 toTarget = targetPos - origin;

            if (toTarget.sqrMagnitude > radius * radius)
                return false;

            float angle = Vector2.Angle(forward, toTarget);
            return angle <= halfAngleDeg;
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
                    Gizmos.DrawWireSphere(transform.position, _data.circleRadius);
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
                    Gizmos.DrawSphere(transform.position, _data.circleRadius);
                    break;

                case AreaShape.Box:
                    Gizmos.DrawCube(transform.position, _data.boxSize);
                    break;
                
                case AreaShape.Cone:
                    DrawConeGizmo(
                        transform.position,
                        transform.right,
                        _data.coneRadius,
                        _data.coneAngle
                    );
                    break;
            }
        }
        
        private void DrawConeGizmo(
            Vector3 origin,
            Vector3 forward,
            float radius,
            float angleDeg)
        {
            int steps = 24;
            float half = angleDeg * 0.5f;

            Vector3 prev = origin;

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                float angle = Mathf.Lerp(-half, half, t);

                Vector3 dir = Quaternion.Euler(0, 0, angle) * forward;
                Vector3 point = origin + dir * radius;

                if (i > 0)
                    Gizmos.DrawLine(prev, point);

                prev = point;
            }

            Gizmos.DrawLine(origin, origin + Quaternion.Euler(0, 0, half) * forward * radius);
            Gizmos.DrawLine(origin, origin + Quaternion.Euler(0, 0, -half) * forward * radius);
        }
#endif
    }
}
