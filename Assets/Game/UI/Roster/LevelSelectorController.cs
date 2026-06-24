using System.Collections.Generic;
using Core.Level;
using Core.Services;
using Core.Services.Save;
using Game.Services.Save;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Roster
{
    public class LevelSelectorController : MonoBehaviour
    {
        [SerializeField] private List<LevelConfig> levels;
        
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI levelNameText;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private GameObject lockedIndicator; // optional lock icon
        
        [Header("Preview Presentation")]
        [SerializeField] private SpriteRenderer previewBackground;
        
        private int _currentIndex;
        private GameSaveManager _saveManager;

        public bool IsCurrentLevelLocked =>
            _saveManager != null && !_saveManager.IsLevelUnlocked(levels[_currentIndex].LevelId);
        public string SelectedSceneName  => levels[_currentIndex].SceneName;

        private void Awake()
        {
            _saveManager = ServiceLocator.Get<ISaveManager>() as GameSaveManager;
            
            previousButton.onClick.AddListener(OnPrevious);
            nextButton.onClick.AddListener(OnNext);

            _currentIndex = 0;
            UpdateDisplay();
        }

        private void OnPrevious()
        {
            _currentIndex = (_currentIndex - 1 + levels.Count) % levels.Count;
            UpdateDisplay();
        }

        private void OnNext()
        {
            _currentIndex = (_currentIndex + 1) % levels.Count;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            bool locked = IsCurrentLevelLocked;
            levelNameText.text = levels[_currentIndex].DisplayName;
            //levelNameText.text = locked ? "???" : levels[_currentIndex].DisplayName;
            
            if (previewBackground != null)
                previewBackground.sprite = levels[_currentIndex].PreviewBackground;
            
            if (lockedIndicator != null)
                lockedIndicator.SetActive(locked);
        }
    }
}
