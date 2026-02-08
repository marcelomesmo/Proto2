using System.Collections.Generic;
using UnityEngine;

namespace Game.Services.Save
{
    // TODO: deprecate this later when we do FTUE to unlock initial character.
    [CreateAssetMenu(fileName = "DefaultProgressionData", menuName = "Game/Save/Default Progression")]
    public sealed class InitialProgressionData : ScriptableObject
    {
        public List<string> startingUnlockedCharacters;
        public int startingGold = 0;
    }
}