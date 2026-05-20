using System;
using Core.Enum;
using Core.Services;
using Core.Services.Meta;
using Game.Services.Meta;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Match
{
    public class EndOfLevelPanel : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("Header")]
        [SerializeField] private TMP_Text resultText;

        [Header("Stats")]
        [SerializeField] private TMP_Text enemiesKilledText;
        [SerializeField] private TMP_Text wavesClearedText;
        [SerializeField] private TMP_Text goldCollectedText;
        [SerializeField] private TMP_Text matchTimeText;

        [Header("Buttons")]
        [SerializeField] private Button quitButton;
        public event Action QuitClicked;

        private void Awake()
        {
            root.SetActive(false);

            quitButton.onClick.AddListener(OnQuitClicked);
        }

        public void Show(
            MatchEndReason reason,
            GameMatchStats  stats,
            float matchDurationSeconds)
        {
            root.SetActive(true);

            RefreshResult(reason);
            RefreshStats(stats, matchDurationSeconds);
        }

        private void RefreshResult(MatchEndReason reason)
        {
            switch (reason)
            {
                case MatchEndReason.Victory:
                    resultText.text = "VICTORY";
                    break;

                case MatchEndReason.Defeat:
                case MatchEndReason.Quit:
                    resultText.text = "DEFEAT";
                    break;
            }
        }

        private void RefreshStats(
            GameMatchStats stats,
            float matchDurationSeconds)
        {
            enemiesKilledText.text =
                $"{stats.EnemiesKilled}";

            wavesClearedText.text =
                $"{stats.WavesCleared}";

            goldCollectedText.text =
                $"{stats.GetGoldCollected()}";

            matchTimeText.text =
                $"{FormatTime(matchDurationSeconds)}";
        }

        private void OnQuitClicked()
        {
            QuitClicked?.Invoke();
        }

        private string FormatTime(float totalSeconds)
        {
            int minutes = Mathf.FloorToInt(totalSeconds / 60f);
            int seconds = Mathf.FloorToInt(totalSeconds % 60f);

            return $"{minutes:00}:{seconds:00}";
        }
    }
}