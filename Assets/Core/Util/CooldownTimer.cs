namespace Core.Util
{
    public class CooldownTimer
    {
        private float _remaining;

        public bool Tick(float deltaTime)
        {
            if (_remaining <= 0f)
                return true;

            _remaining -= deltaTime;

            if (_remaining <= 0f)
                _remaining = 0f;
            
            return _remaining <= 0f;
        }

        public void Start(float duration)
        { 
            _remaining = duration;
        }

        public void Stop() => _remaining = 0f;
        
        public bool IsFinished => _remaining <= 0f;
        public bool IsRunning => _remaining > 0f;
    }
}