namespace Game.Services.Save
{
    public enum PartyChangeResult
    {
        Success,

        InvalidCharacter,
        InvalidPartySize,
        
        SlotLocked,

        CharacterLocked,
        CharacterAlreadyAssigned,
        CharacterNotAssigned,

        PartyAlreadyConfigured,
        PartyFull,
        LastCharacterRequired
    }
}