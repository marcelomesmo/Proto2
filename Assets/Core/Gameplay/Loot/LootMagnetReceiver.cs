using Core.Interfaces;
using UnityEngine;

namespace Core.Gameplay.Loot
{
    public class LootMagnetReceiver : MonoBehaviour, IMagnetizableLoot
    {
        [Header("Magnet")]
        [SerializeField] private float magnetDelay = 0.25f;
        [SerializeField] private float maxSpeed = 8f;

        private Rigidbody2D _rb;
        private bool _canMagnetize;
        private bool _collected;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        } 
        
        private void OnEnable()
        {
            _collected = false;
            _canMagnetize = false;

            Invoke(nameof(EnableMagnet), magnetDelay);
        }
        
        private void OnDisable()
        {
            CancelInvoke();
        }

        private void EnableMagnet()
        {
            _canMagnetize = true;
        }

        public void ApplyMagnet(Vector2 source, float force)
        {
            if (!_canMagnetize || _collected)
                return;
            
            Vector2 dir = (source - _rb.position);
            Vector2 desired = dir.normalized * force;

            _rb.AddForce(desired, ForceMode2D.Force);

            if (_rb.linearVelocity.magnitude > maxSpeed)
                _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;
        }
        
        public void DisablePhysics()
        {
            _collected = true;
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated = false;
        }
    }
}
