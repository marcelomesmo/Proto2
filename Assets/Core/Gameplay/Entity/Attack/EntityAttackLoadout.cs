using System;
using System.Collections.Generic;
using Core.Gameplay.Combat.Attack;
using UnityEngine;

namespace Core.Gameplay.Entity.Attack
{
    public class EntityAttackLoadout : MonoBehaviour
    {
        [SerializeField]
        private List<AttackData> attacks = new();

        public event Action OnLoadoutChanged;

        public IReadOnlyList<AttackData> Attacks => attacks;
        
        public void InitializeFromDefinition(AttackLoadoutDefinition definition)
        {
            attacks.Clear();

            if (!definition)
                return;

            attacks.AddRange(definition.Attacks);
            
            OnLoadoutChanged?.Invoke();
        }
        
        public bool AddAttack(AttackData attack)
        {
            if (attack == null || attacks.Contains(attack))
                return false;

            attacks.Add(attack);
            OnLoadoutChanged?.Invoke();
            return true;
        }

        public bool RemoveAttack(AttackData attack)
        {
            if (attack == null)
                return false;

            OnLoadoutChanged?.Invoke();
            return attacks.Remove(attack);
        }

        public bool HasAttack(AttackData attack)
        {
            return attack != null && attacks.Contains(attack);
        }

        public void Clear()
        {
            attacks.Clear();
        }
    }
}