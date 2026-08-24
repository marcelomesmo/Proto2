using Core.Services;
using Core.Services.Save;
using Game.Services.Save;
using TMPro;
using UnityEngine;

namespace Game.UI.GameMenu
{
    public sealed class GoldDisplayController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI goldText;

        private GameSaveManager _saveManager;

        private void OnEnable()
        {
            _saveManager = ServiceLocator.Get<ISaveManager>() as GameSaveManager;

            if (_saveManager == null)
            {
                Debug.LogError("[GoldDisplayController] GameSaveManager service not found.", this);
                return;
            }

            _saveManager.OnGoldChanged += UpdateDisplay;

            // Important: immediately initialize from the current saved balance.
            UpdateDisplay(_saveManager.Gold);
        }

        private void OnDisable()
        {
            // TODO: Later, if we eventually produce extremely high-frequency currency events (not this project)
            //      we might separate (1) balance/total update immediately vs (2) disk flush periodically.
            //      i.e. we can probably stack loot calls and save once.
            if (_saveManager != null)
            {
                _saveManager.OnGoldChanged -= UpdateDisplay;
            }

            _saveManager = null;
        }

        private void UpdateDisplay(int gold)
        {
            goldText.text = gold.ToString();
        }
    }
}