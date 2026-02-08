namespace Core.Services.Save.Storage
{
    public sealed class LocalStorageProvider : IStorageProvider
    {
        public void Save(string key, string data)
        {
            SaveRepository.Save(key, data);
        }

        public string Load(string key)
        {
            return SaveRepository.Load(key);
        }

        public void Delete(string key)
        {
            SaveRepository.Delete(key);
        }
    }
}