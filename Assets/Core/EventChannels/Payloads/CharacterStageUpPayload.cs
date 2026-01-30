namespace Core.EventChannels.Payloads
{
    public readonly struct CharacterStageUpPayload
    {
        public readonly string characterId;
        public readonly int oldStage;
        public readonly int newStage;

        public CharacterStageUpPayload(string id, int oldStage, int newStage)
        {
            characterId = id;
            this.oldStage = oldStage;
            this.newStage = newStage;
        }
    }

    public readonly struct CharacterStageChangedPayload
    {
        public readonly string characterId;
        public readonly int stage;

        public CharacterStageChangedPayload(string id, int stage)
        {
            characterId = id;
            this.stage = stage;
        }
    }
}