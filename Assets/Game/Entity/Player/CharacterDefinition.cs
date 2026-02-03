using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Game.Entity.Player
{
    [CreateAssetMenu(fileName = "Character Definition", menuName = "Game/New Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        public Sprite rosterScreenPortrait;
        public EntityController prefab;

        public AttackLoadoutDefinition initialAttackLoadout;   // ← definition, not runtime
        public BaseEntityStats baseStats;
    }
}
