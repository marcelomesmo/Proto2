using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Game.Entity.Player
{
    [CreateAssetMenu(fileName = "Castle Definition", menuName = "Game/New Castle Definition")]
    public class CastleDefinition : ScriptableObject
    {
        public EntityController prefab;
        
        public BaseEntityStats baseStats;
    }
}
