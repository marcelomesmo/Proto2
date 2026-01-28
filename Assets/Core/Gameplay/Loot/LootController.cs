using System;
using Core.Enum;
using Core.Interfaces;
using PrimeTween;
using UnityEngine;

// Handle spawn impulse, gravity, and physical bounce only.
namespace Core.Gameplay.Loot
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class LootController : MonoBehaviour, IMagnetizableLoot
    {
        public enum State
        {
            Launching,
            Idle,
            Magnetized,
            Collected
        }
        
        private Rigidbody2D _rb;
        private State _state;
        
        private LootSO _lootData;
        private Tween _magnetTween;
        
        public event Action MagnetArrived;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(LootSO lootData)
        {
            _lootData = lootData;
        }

        private void OnEnable()
        {
            _state = State.Launching;
            
            Debug.Assert(_rb != null, $"{name}: Rigidbody2D missing");
            Debug.Assert(_lootData != null, $"{name}: LootData not initialized");
            
            _rb.simulated = true;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            
            _rb.gravityScale = 
                _lootData.physicsMode == LootPhysicsMode.Platformer ? 2f : 0f;
        }
        
        private void OnDisable()
        {
            if (_magnetTween.isAlive)
                _magnetTween.Stop();
        }

        // -------------------------
        // Launch
        // -------------------------
        
        // Launch loot with an arc. DirectionBias: -1 (left), 0 (neutral), +1 (right)
        public void Launch(Vector2 directionBias, float horizontalForce, float verticalForce)
        {
#if UNITY_EDITOR
            if (_lootData == null)
                Debug.LogError($"{name}: LootController.Initialize was not called.");
#endif
            
            Vector2 impulse = new(
                directionBias.x * horizontalForce,
                verticalForce
            );

            _rb.AddForce(impulse, ForceMode2D.Impulse);
            
            // If no bounce is expected, settle automatically
            if (_lootData.physicsMode != LootPhysicsMode.Platformer || !_lootData.enableBounce)
            {
                Invoke(nameof(SetIdle), _lootData.magnetStartDelay);
            }
        }
        
        private void SetIdle()
        {
            if (_state != State.Launching)
                return;

            _state = State.Idle;
        }

        // -------------------------
        // Bounce (platformer only)
        // -------------------------
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_state != State.Launching)
                return;

            if (!_lootData.enableBounce)
                return;
            
            if (_lootData.physicsMode != LootPhysicsMode.Platformer)
                return;
            
            if (((1 << collision.gameObject.layer) & _lootData.groundMask) == 0)   // If the object I collided with is NOT in the groundMask, ignore it.
                return;                                                             // Prevents bouncing in: walls, enemies, loot, triggers, decorations...
            
            _rb.AddForce(Vector2.up * _lootData.bounceImpulse, ForceMode2D.Impulse);
            _state = State.Idle;
        }
        
        // -------------------------
        // Magnet
        // -------------------------
        
        public void TryBeginMagnet(Transform target, float speedMultiplier)
        {
            if (_state != State.Idle)
                return;

            _state = State.Magnetized;
            _rb.simulated = false;
            
            float distance = Vector2.Distance(transform.position, target.position);
            float duration = distance / (_lootData.baseMagnetSpeed * speedMultiplier);
            //float duration =
            //    _lootData.magnetDuration / Mathf.Max(0.01f, speedMultiplier);

            _magnetTween = Tween.Position(
                transform,
                target.position,
                duration,
                Ease.InQuad
            ).OnComplete(() =>
            {
                _state = State.Collected;
                MagnetArrived?.Invoke();
                
                // todo: replace with pool later?
                Destroy(gameObject);
            });
        }
    }
}
