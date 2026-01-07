using UnityEngine;

namespace Gameplay.Buildings
{
    public class StageTransitionController : MonoBehaviour, IInteractable
    {
        [Header("UI Elements")]
        [Tooltip("Button to display on top of the building")]
        [SerializeField] private GameObject overlayButton;

        public void Interact()
        {
            // No implementation here.
            // Button display only.
        }

        public void StopInteract()
        {
            // No implementation here.
            // Button display only.
        }
    
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                overlayButton.SetActive(true);
        }
    
        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                overlayButton.SetActive(false);
        }
    }
}
