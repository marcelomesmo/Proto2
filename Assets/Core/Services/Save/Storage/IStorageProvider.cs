namespace Core.Services.Save.Storage
{
    public interface IStorageProvider
    {
        void Save(string key, string data);
        string Load(string key);
        void Delete(string key);
    }
}