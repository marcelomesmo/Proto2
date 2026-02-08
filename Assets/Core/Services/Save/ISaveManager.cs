namespace Core.Services.Save
{
    public interface ISaveManager
    {
        void Initialize();
        void Load();
        void Save();
        void Reset();
        void OnMatchEnd();
    }
}
