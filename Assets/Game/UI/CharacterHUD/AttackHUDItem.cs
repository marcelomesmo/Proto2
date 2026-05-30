using Core.Gameplay.Combat.Attack;

namespace Game.UI.CharacterHUD
{
    public sealed class AttackHUDItem
    {
        private readonly AttackInstance _attack;
        private readonly AttackHUDItemView _view;

        public AttackHUDItem(
            AttackInstance attack,
            AttackHUDItemView view)
        {
            _attack = attack;
            _view = view;

            _view.SetIcon(_attack.Data.icon);
        }

        public void Tick()
        {
            float normalized =
                _attack.IsReady
                    ? 1f
                    : 1f - (_attack.CooldownRemaining / _attack.GetCooldown());

            _view.SetCooldown(normalized, _attack.IsReady);
        }
    }
}