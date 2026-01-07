using Enemy.Spawner;
using UnityEngine;

namespace Gameplay.Buildings
{
    public class EnvSpawnerController : MonoBehaviour
    {
        /*
        Simple Controller to spawn entities on map start.
        */
        
        [Header("Spawner")]
        [SerializeField] private EnemySpawner spawner;
        
        [Header("Spawn Settings")]
        [SerializeField] private float spawnTimeDelay;
        [SerializeField] private bool spawnOnGameStart;
        
        [Header("Refresh Settings")]
        [SerializeField] private float refreshTimer;

        private SpawnerState _state;
        private Timer _spawnTimer;
        private Timer _refreshTimer;

        private void Start()
        {
            InitializeTimers();
            
            _state = spawnOnGameStart ? SpawnerState.Ready : SpawnerState.Waiting;
        }
        
        private void Update()
        {
            switch (_state)
            {
                case SpawnerState.Ready:
                    HandleReadyState();
                    break;
                case SpawnerState.Spawning:
                    HandleSpawningState();
                    break;
                case SpawnerState.Refreshing:
                    HandleRefreshingState();
                    break;
            }
        }

        private void HandleReadyState()
        {
            if (_spawnTimer.Tick())
            {
                spawner.StartSpawning();
                _state = HasRefreshTimer() ? SpawnerState.Refreshing : SpawnerState.Spawning;
            }
        }
        
        private void HandleSpawningState()
        {
            // Wait for spawner to complete
            // Add feedback code here case necessary
        }

        private void HandleRefreshingState()
        {
            if (_refreshTimer.Tick())
            {
                spawner.ResetSpawner();
                _state = SpawnerState.Waiting;
                InitializeTimers();
            }
        }

        private void InitializeTimers()
        {
            _spawnTimer = new Timer(spawnTimeDelay);
            _refreshTimer = new Timer(refreshTimer);
        }

        private bool HasRefreshTimer() => refreshTimer > 0;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _state = SpawnerState.Ready;
        }
        
#if UNITY_EDITOR
        private BoxCollider2D boxCollider;
        void OnDrawGizmos()
        {
            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider2D>();

            // Set the gizmo color
            Gizmos.color = new Color(0f, 0f, 1f, 0.1f); // Green with 10% transparency

            // Apply the object's transform to the gizmo drawing space
            // This ensures the gizmo rotates and scales with the object
            Gizmos.matrix = transform.localToWorldMatrix;

            // Draw a solid cube using the collider's size and center
            // Note: Collider2D.size and center are in local space
            Gizmos.DrawCube(boxCollider.offset, boxCollider.size);
        }
#endif
        
        public enum SpawnerState
        {
            Waiting,    // Waiting for trigger or initial state
            Ready,      // Ready to start spawn countdown
            Spawning,   // Currently spawning
            Refreshing  // Waiting for refresh timer
        }

        public class Timer
        {
            private float _currentTime;
            private readonly float _duration;

            public Timer(float duration)
            {
                _duration = duration;
                _currentTime = duration;
            }

            public bool Tick()
            {
                _currentTime -= Time.deltaTime;
                if (_currentTime <= 0)
                {
                    _currentTime = _duration;
                    return true;
                }
                return false;
            }
        }

    }
}
