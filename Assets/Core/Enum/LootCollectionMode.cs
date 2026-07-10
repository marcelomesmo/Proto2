namespace Core.Enum
{
    public enum LootCollectionMode
    {
        // Default value preserves your current behavior:
        // player can collect by collision, and magnet can pull it.
        CollisionAndMagnet = 0,

        // Player/entity collision can collect it.
        Collision = 1,

        // Magnet can collect it.
        // Direct collision collection is rejected.
        Magnet = 2,

        // Collected automatically as soon as it becomes collectable.
        // Does not require collider interaction.
        Automatic = 3
    }

    public static class LootCollectionModeExtensions
    {
        public static bool AllowsCollisionCollection(this LootCollectionMode mode)
        {
            return mode == LootCollectionMode.Collision ||
                   mode == LootCollectionMode.CollisionAndMagnet;
        }

        public static bool AllowsMagnetCollection(this LootCollectionMode mode)
        {
            return mode == LootCollectionMode.Magnet ||
                   mode == LootCollectionMode.CollisionAndMagnet;
        }

        public static bool IsAutomatic(this LootCollectionMode mode)
        {
            return mode == LootCollectionMode.Automatic;
        }

        public static bool RequiresColliderWhenCollectable(this LootCollectionMode mode)
        {
            // Collision needs the trigger collider.
            // Magnet also needs the trigger collider because EntityLootMagnetSubsystem
            // finds loot through Physics2D overlap queries.
            return mode.AllowsCollisionCollection() ||
                   mode.AllowsMagnetCollection();
        }
    }
}