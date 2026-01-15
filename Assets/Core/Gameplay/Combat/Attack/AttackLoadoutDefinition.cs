using System.Collections.Generic;
using UnityEngine;

namespace Core.Gameplay.Combat.Attack
{
    [CreateAssetMenu(menuName = "Game/Attack Loadout Definition")]
    public class AttackLoadoutDefinition : ScriptableObject
    {
        [SerializeField] private List<AttackData> attacks = new();

        public IReadOnlyList<AttackData> Attacks => attacks;
    }
}