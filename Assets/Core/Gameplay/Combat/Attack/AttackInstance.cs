namespace Core.Gameplay.Combat.Attack
{
    public sealed class AttackInstance
    {
        public AttackData Data { get; }
        public float CooldownRemaining { get; private set; }

        public AttackInstance(AttackData data)
        {
            Data = data;
            CooldownRemaining = 0f;
        }

        public bool IsReady => CooldownRemaining <= 0f;

        public void Tick(float deltaTime)
        {
            if (CooldownRemaining > 0f)
                CooldownRemaining -= deltaTime;
        }

        public void Consume()
        {
            CooldownRemaining = Data.cooldown;
        }
    }
}
