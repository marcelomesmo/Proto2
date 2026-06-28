using Core.Services;
using TMPro;
using UnityEngine;

namespace Game.UI.Match
{
    public class GameSpeedButton : MonoBehaviour
    {
        [Header("Button")]
        [SerializeField] private TextMeshProUGUI label;

        private void Start()
        {
            UpdateLabel();
        }

        public void OnClick()
        {
            GameTimeService.CycleSpeed();
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            label.text = $"{Time.timeScale:0}x";
        }
    }
}
