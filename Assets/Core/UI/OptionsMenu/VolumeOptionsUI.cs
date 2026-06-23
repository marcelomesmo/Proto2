using Core.Services;
using UnityEngine;
using UnityEngine.UI;

namespace Core.UI.OptionsMenu
{
    public class VolumeOptionsUI : MonoBehaviour
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        private AudioManager _audio;

        private void Awake()
        {
            _audio = ServiceLocator.Get<AudioManager>();

            // Set values without triggering callbacks
            musicSlider.SetValueWithoutNotify(_audio.MusicVolume);
            sfxSlider.SetValueWithoutNotify(_audio.SfxVolume);
            //musicSlider.value = _audio.MusicVolume;
            //sfxSlider.value = _audio.SfxVolume;

            musicSlider.onValueChanged.AddListener(_audio.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(_audio.SetSfxVolume);
        }
    }
}
