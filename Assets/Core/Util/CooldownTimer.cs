namespace Core.Util
{
    public class CooldownTimer
    {
        private float _remaining;
        private readonly float _duration;

        public bool Tick(float deltaTime)
        {
            if (_remaining <= 0f)
                return true;

            _remaining -= deltaTime;
            return _remaining <= 0f;
        }

        public void Reset() => _remaining = _duration;
    }
}