using Core.Services;
using TMPro;
using UnityEngine;

namespace Game.UI
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
            ServiceLocator.Get<GameController>().CycleGameSpeed();
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            label.text = $"{Time.timeScale:0}x";
        }
    }
}
