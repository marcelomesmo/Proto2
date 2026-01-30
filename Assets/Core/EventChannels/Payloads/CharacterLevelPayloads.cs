namespace Core.EventChannels.Payloads
{
    public readonly struct CharacterLevelChangedPayload
    {
        public readonly string characterId;
        public readonly int level;
        public readonly int xp;

        public CharacterLevelChangedPayload(string id, int level, int xp)
        {
            this.characterId = id;
            this.level = level;
            this.xp = xp;
        }
    }

    public readonly struct CharacterLevelUpPayload
    {
        public readonly string characterId;
        public readonly int oldLevel;
        public readonly int newLevel;

        public CharacterLevelUpPayload(string id, int oldLevel, int newLevel)
        {
            this.characterId = id;
            this.oldLevel = oldLevel;
            this.newLevel = newLevel;
        }
    }
}