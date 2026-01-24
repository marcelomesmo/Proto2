namespace Core.Gameplay.Combat.StatusEffect
{
    public abstract class StatusEffectInstance
    {
        public float RemainingTime { get; protected set; }
    
        public bool IsExpired => RemainingTime <= 0f;
    
        public virtual void OnApply() { }
        public virtual void OnTick(float deltaTime) { }
        public virtual void OnRemove() { }

        public void Tick(float deltaTime)
        {
            RemainingTime -= deltaTime;
            OnTick(deltaTime);
        }
    }
}
