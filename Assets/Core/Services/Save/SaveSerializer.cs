using Core.Services.Save.Storage;
using UnityEngine;

namespace Core.Services.Save
{
    /*
     * A generic wrapper to access SaveRepository passing Game specific <T> data.
     * Takes game-specific GameProfile data.
     */
    public sealed class SaveSerializer<T>
        where T : class, new()
    {
        private readonly SaveDescriptor _descriptor;
        private readonly IStorageProvider _storage;

        public T Data { get; private set; }

        public SaveSerializer(
            SaveDescriptor descriptor,
            IStorageProvider storage)
        {
            _descriptor = descriptor;
            _storage = storage;
        }

        public void Load()
        {
            var json = _storage.Load(_descriptor.localFileName);

            if (string.IsNullOrEmpty(json))
            {
                Data = new T();
                return;
            }

            try
            {
                var obj = JsonUtility.FromJson<T>(json);
                Data = obj ?? new T();
            }
            catch
            {
                Debug.LogWarning("[SaveManager] Corrupted save.");
                Data = new T();
            }
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(Data, true);
            _storage.Save(_descriptor.localFileName, json);
        }

        public void Reset()
        {
            Data = new T();
            _storage.Delete(_descriptor.localFileName);
        }
    }
}
