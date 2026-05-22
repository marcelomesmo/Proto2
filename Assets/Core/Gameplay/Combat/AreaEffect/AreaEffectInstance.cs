using System.Collections;
using System.Collections.Generic;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Gameplay.Combat.AreaEffect
{
    public class AreaEffectInstance : MonoBehaviour
    {
        private EntityController _owner;
        
        private AreaEffectData _data;
        private DamagePayload _payload;
        
        private float _remainingTime;
        private float _tickTimer;
        
        private bool _isEnding;
        
        private readonly List<Collider2D> _hits = new();
        
        private IPoolableVisual[] visuals;      // Audio, VFX, Lights, etc.

        public void Awake()
        {
            visuals = GetComponents<IPoolableVisual>();
        }

        public void Initialize(
            EntityController owner,
            AreaEffectData data,
            DamagePayload payload)
        {
            _owner = owner;
            _data = data;
            _payload = payload;

            _hits.Clear();
            
            _remainingTime = data.duration;
            _tickTimer = Mathf.Max(0.01f, data.tickInterval);
            
            _isEnding = false;
        }

        private void Update()
        {
            if (_isReleased)
                return;
            
            // 1. Owner death handling
            if (!_isEnding && _owner && _owner.IsDead)
            {
                HandleOwnerDeath();
            }
            
            // If ending, skip ticking logic
            if (_isEnding)
                return;
            
            // 2. Lifetime
            _remainingTime -= Time.deltaTime;

            if (_remainingTime <= 0f)
            {
                BeginEndSequence();
                return;
            }

            // 3. Tick damage
            _tickTimer -= Time.deltaTime;
            
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
        
        
        // Owner lifecycle
        private void HandleOwnerDeath()
        {
            // Other sequencing can be added here.
            
            BeginEndSequence();
        }
        
        // End / fade logic
        private void BeginEndSequence()
        {
            if (_isEnding)
                return;

            _isEnding = true;

            // Stop visuals emission
            foreach (var visual in visuals)
                visual?.OnDespawn();

            // If you later add fade animations,
            // replace this with coroutine logic.
            if (_data.fadeOutDuration > 0f)
                StartCoroutine(FadeOutRoutine());
            else
                ReturnToPoolSafe();
        }
        
        private IEnumerator FadeOutRoutine()
        {
            yield return new WaitForSeconds(_data.fadeOutDuration);

            ReturnToPoolSafe();
        }
        
        
        #region Pool
        
        private IObjectPool<AreaEffectInstance> _objectPool;
        private bool _isReleased;
        public void AssignToPool(IObjectPool<AreaEffectInstance> objectPool) => _objectPool = objectPool;    
        private void ReturnToPool() { _isReleased = true; _objectPool.Release(this); }
        public void ReturnToPoolSafe() { if(_isReleased) return; ReturnToPool(); }
        
        #endregion
    
        #region Pool lifecycle helpers
    
        // Called by pool on Get (actionOnGet)
        public void OnSpawn()
        {
            _isReleased = false;
        
            // Re-enable emission
            foreach (var visual in visuals)
                visual?.OnSpawn();
        }

        // Called by pool on Release (actionOnRelease)
        public void OnDespawn()
        {
            _hits.Clear();
            
            _owner = null;
            _data = null;
            _payload = default;
            
            _remainingTime = 0f;
            _tickTimer = 0f;
            
            _isEnding = false;
            
            // Disable emission
            foreach (var visual in visuals)
                visual?.OnDespawn();
        }

        #endregion
        
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
            
            float duration = Mathf.Max(0.0001f, _data.duration);
            float t = Mathf.Clamp01(_remainingTime / duration);
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
