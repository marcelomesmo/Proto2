using System;

namespace Core.Services.Save
{
    [Serializable]
    public sealed class AudioSettingsSave
    {
        public float musicVolume = 0.5f;
        public float sfxVolume = 1f;
    }
}
