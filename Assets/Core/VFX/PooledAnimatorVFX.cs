using UnityEngine;

namespace Core.VFX
{
    public class PooledAnimatorVFX : PooledVFX
    {
        [SerializeField] private float maxDuration = 5f; // safeguard for looping
        
        private Animator _animator;
        private float _elapsed;
        private bool _playing;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            if (!_animator)
            {
                Debug.LogWarning(
                    $"[PooledAnimatorVFX] Missing Animator on '{name}'", this);
                enabled = false;
            }
        }

        private void Update()
        {
            if (!_playing)
                return;

            _elapsed += Time.deltaTime;
            if (_elapsed >= maxDuration)
                OnAnimationFinished();
        }

        public override void OnSpawn()
        {
            base.OnSpawn();
            _elapsed = 0f;
            _playing = true;
        }

        public override void OnDespawn()
        {
            _playing = false;
            _elapsed = 0f;
        }

        // Called by VFXAutoRelease StateMachineBehaviour
        public void OnAnimationFinished()
        {
            if (!_playing) return; // guard against double-release
            _playing = false;
            ReturnToPool();
        }
    }
}
