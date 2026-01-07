using UnityEngine;

namespace Gameplay.Loot
{
    [RequireComponent(typeof(Collider2D))]
    public class LootPickupDelay : MonoBehaviour
    {
        [SerializeField] private float pickupDelay = 0.25f;
        private Collider2D _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            _collider.enabled = false;
            Invoke(nameof(EnablePickup), pickupDelay);
        }

        private void EnablePickup()
        {
            _collider.enabled = true;
        }
    }
}