using System.Collections.Generic;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Game.UI.CharacterHUD
{
    public sealed class CharacterHUDAttacksView : MonoBehaviour
    {
        [SerializeField] private AttackHUDItemView itemPrefab;
        [SerializeField] private Transform root;

        private readonly List<AttackHUDItem> _items = new();

        public void BindAttacks(
            EntityAttackSubsystem attackSubsystem,
            IReadOnlyList<AttackData> orderedAttacks)
        {
            Clear();

            foreach (var attackData in orderedAttacks)
            {
                if (!attackSubsystem.Attacks.TryGetValue(attackData, out var instance))
                    continue;

                var view = Instantiate(itemPrefab, root);
                view.transform.SetAsLastSibling();

                var item = new AttackHUDItem(instance, view);
                _items.Add(item);
            }
        }

        private void Update()
        {
            foreach (var item in _items)
                item.Tick();
        }

        private void Clear()
        {
            foreach (Transform child in root)
                Destroy(child.gameObject);

            _items.Clear();
        }
    }
}