using Core.Audio;
using Core.Audio.Data;
using Core.Services.Manager;
using Core.Services.Save;
using Core.Services.Save.Storage;
using UnityEngine;
using UnityEngine.Audio;

namespace Core.Services
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Config")]
        [SerializeField] private AudioSettingsData settings;
        
        [Header("Settings Persistence")]
        [SerializeField] private SaveDescriptor audioSettingsDescriptor;

        [Header("Mixer")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerGroup uiGroup;
        [SerializeField] private AudioMixerGroup ambientGroup;

        private AudioSourcePoolManager _poolManager;
        private Transform _listener;
        private SaveSerializer<AudioSettingsSave> _settingsSave;

        // Dedicated music source
        public MusicPlayer Music { get; private set; }
        
        // Exposed to UI Menu
        private float _musicVolume = 0.5f;  // linear (0-1)
        private float _sfxVolume = 1f;  // linear (0-1)
        public float MusicVolume => _musicVolume;
        public float SfxVolume => _sfxVolume;
        
        private const float MIN_DB = -80f;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Now this is done explicitly by the CameraController
            //_listener = FindAnyObjectByType<AudioListener>()?.transform;
            
            _poolManager = new AudioSourcePoolManager(settings.initialPoolSize, transform);
            
            Music = gameObject.AddComponent<MusicPlayer>();
            Music.Initialize(musicGroup);
            
            // Same system as our save profile.
            // If audio_settings.json exists on disk it loads it, if it doesn't, it creates a new AudioSettings in memory with default values. 
            _settingsSave = new SaveSerializer<AudioSettingsSave>(
                audioSettingsDescriptor,
                new LocalStorageProvider());
            
            _settingsSave.Load();

            _musicVolume = _settingsSave.Data.musicVolume;
            _sfxVolume = _settingsSave.Data.sfxVolume;
            
            ApplyVolumes(); // sync mixer to initial values immediately
        }
        
        #region Playback
        
        public void Play(AudioEvent audioEvent, Vector3 worldPosition)
        {
            if (audioEvent == null)
                return;

            AudioClip clip = audioEvent.clipSet?.GetRandom();
            if (clip == null)
                return;

            AudioSource source = _poolManager.Get();

            source.transform.position = GetFlattenedPosition(worldPosition);
            source.clip = clip;
            source.outputAudioMixerGroup = ResolveBus(audioEvent.bus, audioEvent.mixerGroup);

            source.volume =
                audioEvent.volume +
                Random.Range(-audioEvent.volumeRandomness, audioEvent.volumeRandomness);

            source.pitch =
                audioEvent.pitch +
                Random.Range(-audioEvent.pitchRandomness, audioEvent.pitchRandomness);

            source.spatialBlend = audioEvent.spatialBlend;
            source.minDistance = audioEvent.minDistance;
            source.maxDistance = audioEvent.maxDistance;
            source.priority = audioEvent.priority;

            source.Play();

            StartCoroutine(ReturnWhenFinished(source));
        }

        private AudioMixerGroup ResolveBus(AudioBus bus, AudioMixerGroup overrideGroup)
        {
            if (overrideGroup != null)
                return overrideGroup;

            return bus switch
            {
                AudioBus.Music => musicGroup,
                AudioBus.UI => uiGroup,
                AudioBus.Ambient => ambientGroup,
                _ => sfxGroup
            };
        }
        
        #endregion
        
        #region Volume Control (Mixer-driven)
        
        public void SetMusicVolume(float value)
        {
            _musicVolume = Mathf.Clamp01(value);
            _settingsSave.Data.musicVolume = _musicVolume;
            _settingsSave.Save();
            ApplyVolumes();
        }

        public void SetSfxVolume(float value)
        {
            _sfxVolume = Mathf.Clamp01(value);
            _settingsSave.Data.sfxVolume = _sfxVolume;
            _settingsSave.Save();
            ApplyVolumes();
        }
        
        private void ApplyVolumes()
        {
            // Music affects Music + Ambient
            audioMixer.SetFloat("MusicVolume", LinearToDb(_musicVolume * _musicVolume));    // TODO: Triple exponential here if slider still doesnt feel right
            audioMixer.SetFloat("AmbientVolume", LinearToDb(_musicVolume * _musicVolume));

            // SFX affects SFX + UI
            audioMixer.SetFloat("SfxVolume", LinearToDb(_sfxVolume * _sfxVolume));
            audioMixer.SetFloat("UiVolume", LinearToDb(_sfxVolume * _sfxVolume));
        }

        private float LinearToDb(float value)
        {
            if (value <= 0.0001f)
                return MIN_DB;

            return Mathf.Log10(value) * 20f;
        }

        #endregion

        #region Utilities
        
        private Vector3 GetFlattenedPosition(Vector3 emitterPos)
        {
            if (!settings.ignoreVerticalDistance)
                return emitterPos;

            Vector3 listenerPos = _listener.transform.position;
            return new Vector3(emitterPos.x, listenerPos.y, listenerPos.z);
        }
        
        private System.Collections.IEnumerator ReturnWhenFinished(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);
            _poolManager.Release(source);
        }
        
        public void ReleaseAll()
        {
            _poolManager.ReleaseAll();
            Music.Stop();
        }
        
        public void BindListener(AudioListener listener)
        {
            _listener = listener.transform;
        }
        
        #endregion
    }
}