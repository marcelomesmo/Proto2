using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Game.Entity.Player
{
    [CreateAssetMenu(fileName = "Character Definition", menuName = "Game/New Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        public string characterId;
        public Sprite portrait;
        public EntityController prefab;

        public AttackLoadoutDefinition initialAttackLoadout;   // ← definition, not runtime
        public BaseEntityStats baseStats;
        //public CharacterStats stats;  // project-specific stats.
    }
}
