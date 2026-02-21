namespace Core.Upgrades.Database
{
    public interface IUpgradeDatabase
    {
        bool TryGet(string id, out UpgradeDefinition definition);
    }
}
