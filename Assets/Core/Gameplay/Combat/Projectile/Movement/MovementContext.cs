using UnityEngine;

// Per-instance runtime movement state. Implementations are created by a ProjectileMovement SO.
namespace Core.Gameplay.Combat.Projectile.Movement
{
    public abstract class MovementContext
    {
        // Called once when projectile is spawned/initialized.
        // range parameter is the world range passed from the spawn caller (BaseProjectile).
        public abstract void Initialize(Rigidbody2D rb, Vector2 direction, float range);

        // Called each FixedUpdate to move the projectile. Use rb.MovePosition() for kinematic motion.
        public abstract void Move(Rigidbody2D rb, Vector2 direction);
    }
}