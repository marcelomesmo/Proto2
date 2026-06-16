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
        
        private GameSaveManager _save;
        
        private void Awake()
        {
            _save = ServiceLocator.Get<ISaveManager>() as GameSaveManager;

            if (_save == null)
            {
                Debug.LogError("[WalletPanelController] No SaveManager registered.");
                enabled = false;
                return;
            }
            
            Refresh();
            
            _save.OnGoldChanged += HandleGoldChanged;
        }
        
        private void OnDestroy()
        {
            if (_save == null)
                return;

            _save.OnGoldChanged -= HandleGoldChanged;
        }
        
        private void HandleGoldChanged(int gold)
        {
            Refresh();
        }
        
        private void Refresh()
        {
            goldText.text = _save.Gold.ToString();
        }

        private void Clear()
        {
            goldText.text = "";
        }

        public void RefreshExternal()
        {
            Refresh();
        }
    }
}
