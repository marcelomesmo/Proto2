namespace Game.Services.Save
{
    public enum PartyChangeResult
    {
        Success,

        InvalidCharacter,
        InvalidPartySize,
        
        SlotLocked,

        CharacterLocked,
        CharacterAlreadyUnlocked,
        CharacterAlreadyAssigned,
        CharacterNotAssigned,
        CharacterResting,
        
        InsufficientGold,
        
        PartyAlreadyConfigured,
        PartyFull,
        LastCharacterRequired
    }
}