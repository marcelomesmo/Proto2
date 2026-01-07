using UnityEngine;

public abstract class ProjectileMovement : ScriptableObject
{
    [Header("Common")]
    public ProjectilePhysicsMode physicsMode = ProjectilePhysicsMode.Dynamic;
    
    // Optional default speed or config for some movement types
    public float speed = 15f;
    
    // Create a new per-instance MovementContext for the projectile.
    // The returned MovementContext will be owned by the BaseProjectile instance.
    public abstract MovementContext CreateContext();
}
