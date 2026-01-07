namespace Enum
{
    public enum ActionState
    {
        Idle,
        Shooting,
        Turning,
        Reloading,
        Interacting
    }

    public enum JumpState
    {
        Grounded,
        PrepareToJump,
        Jumping,
        InFlight,
        Landed
    }
    
    public enum AimState
    {
        Right,
        Left,
        TopRight,
        TopLeft
    }
}