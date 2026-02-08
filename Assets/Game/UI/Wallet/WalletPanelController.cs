using Core.Services;
using Core.Services.Save;
using Game.Services.Save;
using TMPro;
using UnityEngine;

namespace Game.UI.Wallet
{
    public class WalletPanelController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI goldText;
        
        private void Awake()
        {
            Build();
        }

        public void Refresh()
        {
            Build();
        }
        
        private void Build()
        {
            Clear();
            
            var saveManager = ServiceLocator.Get<ISaveManager>() as GameSaveManager;
            if (saveManager == null)
            {
                Debug.LogError("[RosterCharacterButton] No SaveManager registered.");
                return;
            }
            
            goldText.text = saveManager.Profile.gold.ToString();
        }

        private void Clear()
        {
            goldText.text = "";
        }
    }
}
