using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.Projectile.Impact;
using Core.Gameplay.Combat.Projectile.Movement;
using Core.Interfaces;
using Enum;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Gameplay.Combat.Projectile
{
    public class ProjectileInstance : MonoBehaviour
    {
        [Header("Behaviours")]
        public ProjectileMovement movementBehavior;
        public ProjectileImpact impactBehavior;
        public AttackData attackProperties;
        [SerializeField] private ProjectileLifetimePolicy lifetimePolicy = ProjectileLifetimePolicy.DespawnOnLastHit;
        
        [Header("VFX")]
        [SerializeField] protected TrailRenderer trailVFXPrefab;
    
        private Faction _ownerFaction;
        public Faction GetOwnerFaction() => _ownerFaction;
        
        // per-instance impact context created from the SO
        // TODO: We might need to create a local ProjectileContext as well later.
        
        // per-instance movement context created from the SO
        private MovementContext _movementContext;
        private bool _initializedThisLife = false;
        private bool _hasLaunched = false; // Guard to prevent FixedUpdate running before Propel
    
        private Vector2 _startPos = Vector2.zero;
        private Vector2 _direction = Vector2.zero;
        private float _range = 5f;   // How far the projectile will travel in world units
        private float _rangeSqr;     // Optimization: squared range

        // per-instance context created on fire
        private ProjectileContext _context;
        private int _remainingHits;

        private Rigidbody2D rb;
        private Collider2D col;
        private SpriteRenderer sr;
        private DamageSource _damageSource;
        
        public void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            sr = GetComponentInChildren<SpriteRenderer>();
        }

        public void Configure(ProjectileContext context, DamageSource source)
        {
            _ownerFaction = context.faction;
            _damageSource = source;
            
            _remainingHits = impactBehavior.GetMaxTargets(context);

            _range = context.range;
            _rangeSqr = _range * _range;
            
            // cache context if needed later
            _context = context;
        }
        
        // FixedUpdate used for physics/movement in Kinematic objects
        protected virtual void FixedUpdate()
        {
            if (!_hasLaunched) return;
        
            // Initialize movement context on first physics step after spawn.
            if (!_initializedThisLife)
            {
                // Ensure we have a context
                if (_movementContext == null && movementBehavior)
                    _movementContext = movementBehavior.CreateContext();

                _movementContext?.Initialize(rb, _direction, _range);
                _initializedThisLife = true;
            }

            // Movement tick (always call so dynamic movement can operate if needed)
            _movementContext?.Move(rb, _direction);
        
            // Calculate the distance in units to detect the max range
            // Optimization: Use sqrMagnitude to avoid square root calculations in FixedUpdate
            if ((rb.position - _startPos).sqrMagnitude >= _rangeSqr)
            {
                OnMaxRangeReached();
            }
        }

        public void Launch(float inAngleDeg)
        {
            float angleRad = inAngleDeg * Mathf.Deg2Rad;
            _direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        
            // When reusing a pooled projectile, its Rigidbody position might not be in sync with the
            // transform's position, which is set by the weapon. This forces the Rigidbody's
            // position to match the transform's position right before we cache the start position
            // and begin movement. This ensures the range check in FixedUpdate works correctly.
            rb.position = transform.position;
        
            // Rotate the sprite visually to match direction
            float facingAngle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            sr.transform.rotation = Quaternion.Euler(0, 0, facingAngle);
        
            // Flip sprite if shooting left
            if (sr)
                sr.flipY = (_direction.x < 0);
        
            // Check if a behavior is assigned
            if (!movementBehavior)
            {
                Debug.LogWarning("No movementBehavior assigned to projectile.", this);
                return;
            }
        
            // Create a fresh movement context and set physics mode according to the SO
            //_movementContext = movementBehavior.CreateContext();
            _initializedThisLife = false; // will initialize in FixedUpdate
        
            // Check if the prefab has the correct physics setup
            if (movementBehavior.physicsMode == ProjectilePhysicsMode.Kinematic)
            {
                if (rb.bodyType != RigidbodyType2D.Kinematic)
                    Debug.LogWarning($"Projectile prefab {name} must be Kinematic but isn't!");
            }
            else // Dynamic
            {
                if (rb.bodyType != RigidbodyType2D.Dynamic)
                    Debug.LogWarning($"Projectile prefab {name} must be Dynamic but isn't!");
            }
        
            // Init positions// Use transform.position because rb.position might not be synced with the transform 
            // changes made in this frame yet (especially for new/pooled objects).
            _startPos = transform.position;
        
            // Mark as launched so FixedUpdate can proceed
            _hasLaunched = true;

            // Important: do not call _movementContext.Initialize here to avoid ordering
            // issues with pools—defer to FixedUpdate first tick which will call Initialize.
        }
    
        public void Launch(AimState aimState)
        {
            // Direction vector
            float angleDeg = aimState switch
            {
                AimState.Right => 0f, // right
                AimState.TopRight => 30f,
                AimState.TopLeft => 150f,
                AimState.Left => 180f, // left
                _ => 0f
            };

            Launch(angleDeg);
        }
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Prevent processing if already released (e.g. multiple collisions in one frame)
            if (_isReleased) return;

            if (_remainingHits <= 0)
                return;

            // Ignore Damageables that can't be hit.
            if (other.TryGetComponent<IDamageable>(out var damageable) && !damageable.CanBeDamaged())
                return;

            // Get the point on the enemy collider closest to the projectile
            Vector2 hitPoint = other.ClosestPoint(transform.position);
            
            var payload = new DamagePayload(
                hitData: attackProperties, // TODO: In case of damage multipliers, should apply before this.
                effects: attackProperties.Effects,
                hitPoint: hitPoint,
                source: _damageSource
            );
            
            // Trigger OnImpact results
            impactBehavior?.OnImpact(this, other, payload);
            
            _remainingHits--;
        
            // Return to pool based on policy
            if (_remainingHits <= 0 &&
                lifetimePolicy == ProjectileLifetimePolicy.DespawnOnLastHit)
            {
                ReturnToPool();
            }
        }

        private void OnMaxRangeReached()
        {
            if (_isReleased) return;
        
            // Trigger OnMaxRangeReached results
            //Debug.Log("Projectile reached max range.");
        
            // Spawn the VFX BEFORE deactivation
            SpawnMaxRangeVFX();
        
            // Return to pool immediately on despawn
            ReturnToPool();
        }
    
        #region VFX helpers

        private void SpawnImpactVFX(Vector2 impactPoint)
        {
            /*
            if (!impactVFXPrefab)
                return;
        
            // Get pooled VFX
            var vfx = VFXPoolManager.Instance.Spawn(impactVFXPrefab);
        
            Vector3 hitPos = impactPoint;
            Vector3 hitNormal = impactPoint.normalized;
        
            vfx.transform.position = hitPos;
            vfx.transform.rotation = Quaternion.Euler(0, 0,
                Vector2.SignedAngle(Vector2.right, hitNormal));

            vfx.GetComponent<ParticleSystemVFX>()
                .Initialize(impactVFXPrefab);
            */
        }

        private void SpawnMaxRangeVFX()
        {
            // TODO
        }
        #endregion
    
        #region Pool
        
        private IObjectPool<ProjectileInstance> _objectPool;
        private bool _isReleased;
        public void AssignToPool(IObjectPool<ProjectileInstance> objectPool) => _objectPool = objectPool;    
        private void ReturnToPool() { _isReleased = true; _objectPool.Release(this); }
        
        #endregion
    
        #region Pool lifecycle helpers
    
        // Called by pool on Get (actionOnGet)
        public void OnSpawn()
        {
            _isReleased = false;
        
            // Activate (in pool) and ensure physics state is clean. Do not change bodyType here,
            // it will be set by PropelProjectile according to movementBehavior.

            rb.simulated = true;
        
            // ensure sync
            rb.position = transform.position;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            col.enabled = true;

            // Reset flags
            _movementContext = null;
            _initializedThisLife = false;
            _hasLaunched = false; // Reset launch state
        
            // Re-enable emission
            if(trailVFXPrefab)
                trailVFXPrefab.emitting = true;
        }

        // Called by pool on Release (actionOnRelease)
        public void OnDespawn()
        {
            // Deactivate (in pool) and ensure physics state is reset.

            rb.simulated = false;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            col.enabled = false;

            // clear movement context
            _movementContext = null;
            _initializedThisLife = false;
            _hasLaunched = false;

            if (trailVFXPrefab)
            {
                // Stop emission so no new vertices are added
                trailVFXPrefab.emitting = false;
                // Clear existing trail data
                trailVFXPrefab.Clear();
            }

            _remainingHits = 0;
        }

        #endregion
    }
}
