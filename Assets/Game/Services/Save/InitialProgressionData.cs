using System.Collections.Generic;
using Core.Upgrades;
using UnityEngine;

namespace Game.Services.Save
{
    // TODO: deprecate this later when we do FTUE to unlock initial character.
    // PS: We might still want to use this for starting gold/resources and/or initial upgrades, right?
    [CreateAssetMenu(fileName = "DefaultProgressionData", menuName = "Game/Save/Default Progression")]
    public sealed class InitialProgressionData : ScriptableObject
    {
        public List<string> startingUnlockedCharacters;
        public List<UpgradeDefinition> startingUnlockedUpgrades;
        public int startingGold = 0;
    }
}