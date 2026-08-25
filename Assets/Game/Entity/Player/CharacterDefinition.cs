using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Stats;
using UnityEngine;

namespace Game.Entity.Player
{
    [CreateAssetMenu(fileName = "Character Definition", menuName = "Game/New Character Definition")]
    public class CharacterDefinition : ScriptableObject
    {
        [Header("Meta")]
        [Min(0)] public int unlockCost;
        
        [Header("Presentation")]
        public Sprite rosterScreenPortrait;
        public GameObject previewPrefab;
        
        [Header("Runtime")]
        public EntityController prefab;
        public AttackLoadoutDefinition initialAttackLoadout;   // ← definition, not runtime
        public BaseEntityStats baseStats;
    }
}
