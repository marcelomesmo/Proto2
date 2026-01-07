using Audio;
using Services;
using Services.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace UI.OptionsMenu
{
    public class VolumeOptionsUI : MonoBehaviour
    {
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;

        private AudioManager _audio;

        private void Awake()
        {
            _audio = ServiceLocator.Get<AudioManager>();

            musicSlider.value = _audio.MusicVolume;
            sfxSlider.value = _audio.SfxVolume;

            musicSlider.onValueChanged.AddListener(_audio.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(_audio.SetSfxVolume);
        }
    }
}
