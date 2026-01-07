using UnityEngine;

// Handle spawn impulse, gravity, and physical bounce only.
namespace Core.Gameplay.Loot
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class LootBounceController : MonoBehaviour
    {
        [Header("Launch")]
        [SerializeField] private float horizontalForce = 2f;
        [SerializeField] private float verticalForce = 12f;

        [Header("Bounce")]
        [SerializeField] private float bounceImpulse = 6f;
        [SerializeField] private LayerMask groundMask;

        private Rigidbody2D _rb;
        private bool _hasBounced;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            _hasBounced = false;
            _rb.linearVelocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }

        // Launch loot with an arc. DirectionBias: -1 (left), 0 (neutral), +1 (right)
        public void Launch(Vector2 directionBias)
        {
            Vector2 impulse = new Vector2(
                directionBias.x * horizontalForce,
                verticalForce
            );

            _rb.AddForce(impulse, ForceMode2D.Impulse);
        }
        
        public void LaunchWithForceOverride(Vector2 directionBias, float horizontalForceOverride, float verticalForceOverride)
        {
            Vector2 impulse = new Vector2(
                directionBias.x * horizontalForceOverride,
                verticalForceOverride
            );

            _rb.AddForce(impulse, ForceMode2D.Impulse);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_hasBounced)
                return;

            if (((1 << collision.gameObject.layer) & groundMask) == 0)   // If the object I collided with is NOT in the groundMask, ignore it.
                return;                                                     // Prevents bouncing in: walls, enemies, loot, triggers, decorations...

            _hasBounced = true;

            // Small bounce impulse upward
            _rb.AddForce(Vector2.up * bounceImpulse, ForceMode2D.Impulse);
            
            // Optional: disable physics after bounce
            //Invoke(nameof(DisablePhysics), 0.15f);
        }
        
        private void DisablePhysics()
        {
            _rb.simulated = false;
        }
    }
}
