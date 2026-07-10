using System;
using System.Collections;
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
            Collectable,
            Magnetized,
            Collected
        }
        
        private Rigidbody2D _rb;
        private LootSO _lootData;
        private State _state;
        
        private Coroutine _collectableDelayRoutine;
        
        private Tween _magnetTween;

        public event Action CollectableBecameAvailable;
        public event Action MagnetStarted;
        public event Action MagnetArrived;

        public State CurrentState => _state;
        public bool HasBecomeCollectable { get; private set; }
        public bool IsCollectable => _state == State.Collectable;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            
            // Do not initialize from OnEnable.
            // BaseLoot owns data initialization and calls Initialize(lootData).
            //
            // This avoids script execution order problems where LootController.OnEnable
            // could run before BaseLoot.OnEnable assigns loot data.
        }
        
        private void OnDisable()
        {
            StopCollectableDelay();
            
            if (_magnetTween.isAlive)
                _magnetTween.Stop();
        }

        public void Initialize(LootSO lootData)
        {
            _lootData = lootData;
            
#if UNITY_EDITOR
            Debug.Assert(_rb != null, $"{name}: Rigidbody2D missing.", this);
            Debug.Assert(_lootData != null, $"{name}: LootData not initialized.", this);
#endif
            
            StopCollectableDelay();

            HasBecomeCollectable = false;
            _state = State.Launching;
            
            if (!_rb)
                return;
            
            _rb.simulated = true;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            
            _rb.gravityScale = 
                _lootData.physicsMode == LootPhysicsMode.Platformer ? 2f : 0f;
        }

        // -------------------------
        // Launch
        // -------------------------
        
        // Launch loot with an arc. DirectionBias: -1 (left), 0 (neutral), +1 (right)
        public void Launch(Vector2 directionBias, float horizontalForce, float verticalForce)
        {
            if (!_lootData)
            {
                Debug.LogError($"{name}: LootController.Initialize was not called before Launch.", this);
                return;
            }

            if (!_rb)
            {
                Debug.LogError($"{name}: Rigidbody2D missing.", this);
                return;
            }

            if (_state != State.Launching)
                return;
            
            Vector2 impulse = new(
                directionBias.x * horizontalForce,
                verticalForce
            );

            _rb.AddForce(impulse, ForceMode2D.Impulse);
            
            bool waitsForPlatformerBounce =
                _lootData.physicsMode == LootPhysicsMode.Platformer &&
                _lootData.enableBounce;
            
            if (!waitsForPlatformerBounce)
                BeginCollectableDelay(_lootData.collectableDelay);
        }
        
        private void BeginCollectableDelay(float delay)
        {
            StopCollectableDelay();

            if (delay <= 0f)
            {
                SetCollectable();
                return;
            }

            _collectableDelayRoutine = StartCoroutine(SetCollectableAfterDelay(delay));
        }
        
        private IEnumerator SetCollectableAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            _collectableDelayRoutine = null;
            SetCollectable();
        }

        private void StopCollectableDelay()
        {
            if (_collectableDelayRoutine == null)
                return;

            StopCoroutine(_collectableDelayRoutine);
            _collectableDelayRoutine = null;
        }
        
        private void SetCollectable()
        {
            if (_state != State.Launching && _state != State.Magnetized)
                return;

            HasBecomeCollectable = true;
            _state = State.Collectable;

            CollectableBecameAvailable?.Invoke();
        }

        // -------------------------
        // Bounce (platformer only)
        // -------------------------
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_lootData)
                return;
            
            if (_state != State.Launching)
                return;

            if (!_lootData.enableBounce)
                return;
            
            if (_lootData.physicsMode != LootPhysicsMode.Platformer)
                return;
            
            if (((1 << collision.gameObject.layer) & _lootData.groundMask) == 0)   // If the object I collided with is NOT in the groundMask, ignore it.
                return;                                                             // Prevents bouncing in: walls, enemies, loot, triggers, decorations...
            
            _rb.AddForce(Vector2.up * _lootData.bounceImpulse, ForceMode2D.Impulse);
            
            SetCollectable();
        }
        
        // -------------------------
        // Magnet
        // -------------------------
        
        public bool TryBeginMagnet(Transform target, float speedMultiplier)
        {
            if (!_lootData.collectionMode.AllowsMagnetCollection())
                return false;

            if (_state != State.Collectable)
                return false;

            _state = State.Magnetized;
            
            _rb.simulated = false;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            
            float distance = Vector2.Distance(transform.position, target.position);
            
            float speed = _lootData.baseMagnetSpeed * Mathf.Max(0.01f, speedMultiplier);
            float duration = distance / Mathf.Max(0.01f, speed);

            if (_magnetTween.isAlive)
                _magnetTween.Stop();
            
            if (duration <= 0f)
            {
                MagnetArrived?.Invoke();
                return true;
            }
            
            _magnetTween = Tween.Position(
                transform,
                target.position,
                duration,
                Ease.InQuad
            ).OnComplete(() =>
            {
                MagnetArrived?.Invoke();
            });
            
            MagnetStarted?.Invoke();

            return true;
        }
        
        // -------------------------
        // Collection finalization
        // -------------------------

        public void MarkCollected()
        {
            StopCollectableDelay();

            if (_magnetTween.isAlive)
                _magnetTween.Stop();
            
            _state = State.Collected;

            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.simulated = false;
        }
    }
}
