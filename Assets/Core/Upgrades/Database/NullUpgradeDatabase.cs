namespace Core.Upgrades.Database
{
    public sealed class NullUpgradeDatabase : IUpgradeDatabase
    {
        public bool TryGet(string id, out UpgradeDefinition definition)
        {
            definition = null;
            return false;
        }
    }
}