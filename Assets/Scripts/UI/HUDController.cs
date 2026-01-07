using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Primary Weapon")]
        [SerializeField] private TextMeshProUGUI primaryAmmoText;
        [SerializeField] private IntIntEventChannelSO primaryAmmoEvent;
        [SerializeField] private WeaponDataEventChannelSO primaryWeaponChangeEvent;
        private GameObject _primaryAmmoUIPrefab;
        [SerializeField] private Image primaryAmmoIcon;
        private Image _primaryProjectileUIImage = null;
    
        [Header("Secondary Weapon")]
        [SerializeField] private TextMeshProUGUI secondaryAmmoText;
        [SerializeField] private IntIntEventChannelSO secondaryAmmoEvent;
        [SerializeField] private IntFloatEventChannelSO secondaryAmmoReloadEvent;    // Reload progress
        [SerializeField] private Transform secondaryAmmoHUDContainer;   // Where to draw the ammo
        [SerializeField] private WeaponDataEventChannelSO secondaryWeaponChangeEvent;
        private GameObject _secondaryAmmoUIPrefab;
        private List<Image> _projectileUIImages = new();
    
        [Header("Wallet")]
        [SerializeField] private TextMeshProUGUI resource1Text;
        [SerializeField] private IntEventChannelSO resource1EventChannel;
        private int _totalResource1Contribution = 0;
        [SerializeField] private TextMeshProUGUI resource2Text;
        [SerializeField] private IntEventChannelSO resource2EventChannel;
        private int _totalResource2Contribution = 0;
        [SerializeField] private TextMeshProUGUI resource3Text;
        [SerializeField] private IntEventChannelSO resource3EventChannel;
        private int _totalResource3Contribution = 0;
    
        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private IntIntEventChannelSO healthEventChannel;
        [SerializeField] private UI_ProgressBar healthBar;
        [SerializeField] private FloatEventChannelSO sprintEventChannel;
        [SerializeField] private UI_ProgressBar sprintBar;

        /* Boilerplate for Event Channel */
        private void OnEnable()
        {
            primaryWeaponChangeEvent.OnEventRaised += SetupPrimaryWeaponHUD;
            primaryAmmoEvent.OnEventRaised += UpdatePrimaryAmmoDisplay;
            secondaryWeaponChangeEvent.OnEventRaised += SetupSecondaryWeaponHUD;
            secondaryAmmoEvent.OnEventRaised += UpdateSecondaryAmmoDisplay;
            secondaryAmmoReloadEvent.OnEventRaised += UpdateAmmoInHUD;
            resource1EventChannel.OnEventRaised += UpdateResource1Display;
            resource2EventChannel.OnEventRaised += UpdateResource2Display;
            resource3EventChannel.OnEventRaised += UpdateResource3Display;
            healthEventChannel.OnEventRaised += UpdateHealthDisplay;
            sprintEventChannel.OnEventRaised += UpdateSprintDisplay;
        }

        private void OnDisable()
        {
            primaryWeaponChangeEvent.OnEventRaised -= SetupPrimaryWeaponHUD;
            primaryAmmoEvent.OnEventRaised -= UpdatePrimaryAmmoDisplay;
            secondaryWeaponChangeEvent.OnEventRaised -= SetupSecondaryWeaponHUD;
            secondaryAmmoEvent.OnEventRaised -= UpdateSecondaryAmmoDisplay;
            secondaryAmmoReloadEvent.OnEventRaised -= UpdateAmmoInHUD;
            resource1EventChannel.OnEventRaised -= UpdateResource1Display;
            resource2EventChannel.OnEventRaised -= UpdateResource2Display;
            resource3EventChannel.OnEventRaised -= UpdateResource3Display;
            healthEventChannel.OnEventRaised -= UpdateHealthDisplay;
            sprintEventChannel.OnEventRaised -= UpdateSprintDisplay;
        }
        /* End of Boilerplate */

        private void UpdatePrimaryAmmoDisplay(int currentAmmo, int maxAmmo)
        {
            primaryAmmoText.text = $"{currentAmmo} / {maxAmmo}";
        }

        private void UpdateSecondaryAmmoDisplay(int currentAmmo, int maxAmmo)
        {
            secondaryAmmoText.text = $"{currentAmmo} / {maxAmmo}";
        }

        private void UpdateResource1Display(int contributionValue)
        {
            _totalResource1Contribution += contributionValue;
            resource1Text.text = $"{_totalResource1Contribution}";
        }
        private void UpdateResource3Display(int contributionValue)
        {
            _totalResource2Contribution += contributionValue;
            resource2Text.text = $"{_totalResource2Contribution}";
        }
        private void UpdateResource2Display(int contributionValue)
        {
            _totalResource3Contribution += contributionValue;
            resource3Text.text = $"{_totalResource3Contribution}";
        }

        private void SetupPrimaryWeaponHUD(WeaponData data)
        {
            _primaryAmmoUIPrefab = data.projectileUIPrefab;
            primaryAmmoIcon.sprite = _primaryAmmoUIPrefab.GetComponent<Image>().sprite;
        }

        private void SetupSecondaryWeaponHUD(WeaponData data)
        {
            _secondaryAmmoUIPrefab = data.projectileUIPrefab;
        
            // Clear any UI image, case exist
            foreach (Transform child in secondaryAmmoHUDContainer)
                Destroy(child.gameObject);

            _projectileUIImages.Clear();

            // Create new UI images
            for (int i = 0; i < data.ammoCapacity; i++)
            {
                GameObject ammoObj = Instantiate(_secondaryAmmoUIPrefab, secondaryAmmoHUDContainer); // Create new prefab w/ image in the hud container position
                Image img = ammoObj.GetComponent<Image>();
                _projectileUIImages.Add(img);   // Store reference to the Images (avoids going through the prefabs and using GetComponent when iterating in update)
            }
        }

        // Update the Secondary Weapon ammo images in HUD to match their reload progress.
        // health = whole hearts (e.g., 3)
        // progress = 0–1 value filling the next heart
        // Example:
        //      health = 3, progress = 0.5 → three full hearts + one heart at 50%
        private void UpdateAmmoInHUD(int currAmmo, float currReloadProgress)
        {
            for (int i = 0; i < _projectileUIImages.Count; i++)
            {
                if (i < currAmmo)
                {
                    // Full heart
                    _projectileUIImages[i].fillAmount = 1f;
                }
                else if (i == currAmmo)
                {
                    // The heart currently being filled
                    _projectileUIImages[i].fillAmount = 1-currReloadProgress;
                }
                else
                {
                    // Empty heart
                    _projectileUIImages[i].fillAmount = 0f;
                }
            }
        }
    
        private void UpdateHealthDisplay(int currHealth, int maxHealth)
        {
            healthText.text = $"{currHealth} / {maxHealth}";
            healthBar.SetProgress(currHealth / (float)maxHealth);
        }

        private void UpdateSprintDisplay(float sprintProgress)
        {
            sprintBar.SetProgress(sprintProgress);
        }
    }
}
