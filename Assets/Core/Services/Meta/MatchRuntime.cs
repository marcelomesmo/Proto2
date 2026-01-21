namespace Core.Services.Meta
{
    public sealed class MatchRuntime
    {
        public float ElapsedTime { get; private set; }
        public bool IsRunning { get; private set; }
        
        public void BeginMatch()
        {
            ElapsedTime = 0f;
            IsRunning = true;
        }

        public void EndMatch()
        {
            IsRunning = false;
        }

        public void Tick(float deltaTime)
        {
            if (!IsRunning) return;
            ElapsedTime += deltaTime;
        }
    }
}
