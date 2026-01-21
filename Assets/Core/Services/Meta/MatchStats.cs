namespace Core.Services.Meta
{
    public sealed class MatchStats
    {
        public int EnemiesKilled { get; private set; }
        public float MatchTime { get; private set; }

        public event System.Action<int> OnEnemiesKilledChanged;
        public event System.Action<float> OnMatchTimeUpdated;

        private bool _running;

        public void StartMatch()
        {
            EnemiesKilled = 0;
            MatchTime = 0f;
            _running = true;
        }

        public void StopMatch()
        {
            _running = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_running)
                return;

            MatchTime += deltaTime;
            OnMatchTimeUpdated?.Invoke(MatchTime);
        }

        public void RegisterEnemyKilled()
        {
            EnemiesKilled++;
            OnEnemiesKilledChanged?.Invoke(EnemiesKilled);
        }
    }
}
