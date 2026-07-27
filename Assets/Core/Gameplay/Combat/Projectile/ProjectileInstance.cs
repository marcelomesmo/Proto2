using Core.Enum;
using Core.Gameplay.Combat.Attack;
using Core.Gameplay.Combat.Projectile.Impact;
using Core.Gameplay.Combat.Projectile.Movement;
using Core.Gameplay.Entity;
using Core.Interfaces;
using Core.Services.Manager;
using Core.VFX;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Gameplay.Combat.Projectile
{
    public class ProjectileInstance : MonoBehaviour
    {
        [Header("Behaviours")]
        public ProjectileMovement movementBehavior;
        public ProjectileImpact impactBehavior;
        [SerializeField] private ProjectileLifetimePolicy lifetimePolicy = ProjectileLifetimePolicy.DespawnOnLastHit;
        
        [Header("VFX")]
        [SerializeField] private PooledVFX trailPrefab;
        private PooledParticleSystemVFX _activeTrail;
        
        //[Header("VFX")]
        //[SerializeField] protected TrailRenderer trailVFXPrefab;
    
        private Faction _ownerFaction;
        public Faction GetOwnerFaction() => _ownerFaction;
        
        // per-instance movement context created from the SO
        private MovementContext _movementContext;
        private bool _initializedThisLife = false;
        private bool _hasLaunched = false; // Guard to prevent FixedUpdate running before Propel
    
        private Vector2 _startPos = Vector2.zero;
        private Vector2 _direction = Vector2.zero;
        private float _range = 5f;   // How far the projectile will travel in world units
        private float _rangeSqr;     // Optimization: squared range
        
        // per-instance impact context created from the SO
        // TODO: We might need to create a local ProjectileContext as well later.
        
        // per-instance context created on fire
        private ProjectileContext _context;
        private int _remainingHits;

        private Rigidbody2D rb;
        private Collider2D col;
        private SpriteRenderer sr;
        private AttackSource _attackSource;
        private CombatPayload _payload;
        private IPoolableVisual[] visuals;      // Audio, VFX, Lights, etc.
        private EntityController _target;
        
        public void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            sr = GetComponentInChildren<SpriteRenderer>();
            visuals = GetComponents<IPoolableVisual>();
        }

        public void Configure(ProjectileContext context, AttackSource source, CombatPayload payload, EntityController target)
        {
            if (source == null || source.targetFilter == null)
            {
                Debug.LogError(
                    $"[ProjectileInstance] Projectile '{name}' configured without a valid DamageSource/TargetFilter",
                    this);
                enabled = false;
                return;
            }
            
            _ownerFaction = context.faction;
            _attackSource = source;
            _payload = payload;
            _target = target;
            
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
                
                // TODO: Have targets added to projectiles
                if (_movementContext is IProjectileTargetReceiver targetReceiver)
                {
                    targetReceiver.SetTarget(
                        _target
                            ? _target.transform
                            : null);
                }
                
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
            
            if (trailPrefab)
            {
                var spawned = VFXPoolManager.Instance.Spawn(trailPrefab, transform.position, transform.rotation);
                _activeTrail = spawned as PooledParticleSystemVFX;

                if (_activeTrail)
                    _activeTrail.transform.SetParent(transform, true);
                else
                    Debug.LogWarning($"[ProjectileInstance] trailPrefab on '{name}' is not a PooledParticleSystemVFX.", this);
            }
        }
    
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Prevent processing if already released (e.g. multiple collisions in one frame)
            if (_isReleased)
                return;

            if (_remainingHits <= 0)
                return;
            
            var filter = _attackSource.targetFilter;
            if (!filter.CanHit(other))
                return;

            // Ignore Damageables that can't be hit.
            if (other.TryGetComponent<ICombatReceiver>(out var damageable) && !damageable.CanReceiveCombat(_payload))
                return;
         
            // Handle Damage done
            // Trigger OnImpact results.
            impactBehavior?.OnImpact(this, other, _payload);
            
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

        private void SpawnMaxRangeVFX()
        {
            // TODO
        }
        #endregion
    
        #region Pool
        
        private IObjectPool<ProjectileInstance> _objectPool;
        private bool _isReleased;
        public void AssignToPool(IObjectPool<ProjectileInstance> objectPool) => _objectPool = objectPool;    
        private void ReturnToPool() { 
            _isReleased = true;
            DetachTrail(); 
            _objectPool.Release(this); 
        }
        
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
            foreach (var visual in visuals)
                visual?.OnSpawn();
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

            // disable emission of vfx that lives with the projectile
            foreach (var visual in visuals)
                visual?.OnDespawn();
            
            _remainingHits = 0;
        }
        
        private void DetachTrail()
        {
            if (!_activeTrail)
                return;
            
            _activeTrail.transform.SetParent(VFXPoolManager.Instance.transform, true);
            _activeTrail.StopEmitting();
            _activeTrail = null;
        }

        #endregion
    }
}
