using System.Collections.Generic;
using Game.Entity.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Roster
{
    public class PartyPreviewController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private PlayerLoadoutData loadout;

        [Header("Rendering")]
        [SerializeField] private Camera previewCamera;
        [SerializeField] private int slotResolution = 256;
        [SerializeField] private TMP_Text partyCountText;

        [Header("Slots")]
        [SerializeField] private RawImage previewDisplay;
        [SerializeField] private List<Transform> slotAnchors;

        private RenderTexture _renderTexture;
        private readonly List<GameObject> _activePreviewInstances = new();

        private int _maxSlots;
        private int _selectedSlots;
        
        private void Awake()
        {
            _maxSlots = loadout.MaxPartySize;
            ValidateSlotSetup(_maxSlots);
            BuildRenderTexture(_maxSlots);
            previewDisplay.enabled = true;
        }
        
        private void OnDestroy()
        {
            ClearPreviews();

            if (_renderTexture == null)
                return;

            if (previewCamera)
                previewCamera.targetTexture = null;

            _renderTexture.Release();
            Destroy(_renderTexture);
        }
        
        // --------------------------------------------------
        // Public API — called by RosterPanelController
        // --------------------------------------------------

        public void Refresh()
        {
            ClearPreviews();

            var characters = loadout.PartySlots;

            for (int i = 0; i < slotAnchors.Count; i++)
            {
                if (i >= characters.Count)
                    continue;

                var character = characters[i];

                if (character.previewPrefab == null)
                {
                    Debug.LogWarning(
                        $"[PartyPreviewController] '{character.name}' has no previewPrefab assigned.");
                    continue;
                }

                var instance = Instantiate(
                    character.previewPrefab,
                    slotAnchors[i].position,
                    Quaternion.identity);

                var animator = instance.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.Play("Character-Idle", 0, 0f);
                    animator.Update(0f);
                }
                
                // Flip to face right
                var sr = instance.GetComponent<SpriteRenderer>();
                if (sr != null)
                    sr.flipX = true;
                
                _activePreviewInstances.Add(instance);
            }
            
            _selectedSlots = characters.Count;
            partyCountText.text = $"Party [{_selectedSlots}/{_maxSlots}]";
            if (_selectedSlots == 0)
                partyCountText.color = Color.darkGray;
            else
                partyCountText.color = Color.white;

        }
        
        // --------------------------------------------------
        // Internal
        // --------------------------------------------------

        private void BuildRenderTexture(int slotCount)
        {
            int width  = slotResolution * slotCount;
            int height = slotResolution;

            _renderTexture = new RenderTexture(width, height, 16)
            {
                filterMode = FilterMode.Bilinear,
                antiAliasing = 1
            };

            _renderTexture.Create();
            previewCamera.targetTexture = _renderTexture;
            previewCamera.aspect = (float)width / height;
            
            previewDisplay.texture = _renderTexture;
        }

        private void ClearPreviews()
        {
            foreach (var instance in _activePreviewInstances)
            {
                if (instance != null)
                    Destroy(instance);
            }

            _activePreviewInstances.Clear();
        }

        private void ValidateSlotSetup(int maxSlots)
        {
            if (slotAnchors.Count != maxSlots)
                Debug.LogWarning(
                    $"[PartyPreviewController] slotAnchors count ({slotAnchors.Count}) " +
                    $"doesn't match maxPartySize ({maxSlots}).");
        }
    }
}
