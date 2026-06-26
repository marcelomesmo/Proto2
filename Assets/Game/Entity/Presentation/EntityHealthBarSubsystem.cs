using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Game.Entity.Presentation
{
    public class EntityHealthBarSubsystem : BaseSubsystem
    {
        private EntityHealth healthSubsystem;
        
        [Header("Renderers")]
        [SerializeField] private SpriteRenderer fillRenderer;
        [SerializeField] private SpriteRenderer chipRenderer;

        [Header("Smoothing")]
        [SerializeField] private float fillSmoothSpeed = 8f;
        [SerializeField] private float chipDelay = 0.25f;
        [SerializeField] private float chipSmoothSpeed = 4f;

        [Header("Visibility")]
        [SerializeField] private float hideAfterSeconds = 2.5f;

        private float _fullWidth;

        private float _targetFill;
        private float _currentFill;
        private float _chipFill;

        private float _chipTimer;
        private float _hideTimer;

        private bool _isVisible;
        
        private bool _hasReceivedDamage;
    
        protected override void OnInitialize()
        {
            healthSubsystem = GetComponent<EntityHealth>();
            healthSubsystem.HealthChanged += OnHealthChanged;
            
            _fullWidth = fillRenderer.size.x;
            
            // Initialize full health at start always.
            _targetFill = _currentFill = _chipFill = 1f;

            ApplyFill();
            ApplyChip();
            
            _isVisible = true; // force SetVisible to run
            SetVisible(false);

            _hideTimer = 0f;
        }

        protected override void OnDeinitialize()
        {
            fillRenderer.size =
                new Vector2(_fullWidth, fillRenderer.size.y);
            
            healthSubsystem.HealthChanged -= OnHealthChanged;
            healthSubsystem = null;
        }
        
        private void Update()
        {
            AnimateFill();
            AnimateChip();
            UpdateVisibility();
        }

        private void OnHealthChanged(int current, int max)
        {
            _targetFill = Mathf.Clamp01((float)current / max);
            
            // Reset inactivity timer on ANY health change
            _hideTimer = hideAfterSeconds;

            // Show immediately on damage
            SetVisible(true);

            // Reset chip delay when taking damage
            if (_targetFill < _chipFill)
                _chipTimer = chipDelay;
            else
                _chipFill = _targetFill;    // Protects against healing received: 100 -> 20 hp, chip waiting at 100, then heal to 70. tldr: healing always makes chip catch up.
        }

        private void AnimateFill()
        {
            float delta = Mathf.Abs(_targetFill - _currentFill);
            float speed = Mathf.Max(delta * 10f, fillSmoothSpeed);
            
            _currentFill = Mathf.MoveTowards(
                _currentFill,
                _targetFill,
                speed * Time.deltaTime
            );

            ApplyFill();
        }

        private void AnimateChip()
        {
            if (_chipTimer > 0f)
            {
                _chipTimer -= Time.deltaTime;
                return;
            }

            _chipFill = Mathf.MoveTowards(
                _chipFill,
                _targetFill,
                chipSmoothSpeed * Time.deltaTime
            );

            ApplyChip();
        }

        private void UpdateVisibility()
        {
            if (!_isVisible)
                return;

            // Full health → hide (this was bypassing the timer and automatically hiding while at full health for any entity Player/Enemy, I commented to avoid it hiding too quick, let the timer handle it)
            /*if (_targetFill >= 1f)
            {
                SetVisible(false);
                return;
            }*/

            _hideTimer -= Time.deltaTime;
            
            // Inactivity → hide
            if (_hideTimer <= 0f)
            {
                SetVisible(false);
            }
        }

        private void SetVisible(bool visible)
        {
            if (_isVisible == visible)
                return;

            _isVisible = visible;

            fillRenderer.enabled = visible;
            chipRenderer.enabled = visible;
        }

        private void ApplyFill()
        {
            fillRenderer.size =
                new Vector2(_fullWidth * _currentFill, fillRenderer.size.y);
        }

        private void ApplyChip()
        {
            chipRenderer.size =
                new Vector2(_fullWidth * _chipFill, chipRenderer.size.y);
        }
    }
}
