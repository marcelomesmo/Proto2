using Core.Gameplay.Combat.Attack;

namespace Core.Gameplay.Combat.StatusEffect
{
    public abstract class StatusEffectInstance
    {
        public StatusEffectData SourceData { get; protected set; }  // set in each subclass constructor
        public float RemainingTime { get; protected set; }
        public bool IsPermanent => SourceData?.isPermanent ?? false;
        public bool IsExpired => !IsPermanent && RemainingTime <= 0f;
    
        public virtual void OnApply() { }
        public virtual void OnTick(float deltaTime) { }
        public virtual void OnRemove() { }

        public void Tick(float deltaTime)
        {
            if(!IsPermanent)
                RemainingTime -= deltaTime;
            OnTick(deltaTime);
        }
        
        public void RefreshDuration()
        {
            if(!IsPermanent)
                RemainingTime = SourceData.duration;
        }
    }
}
