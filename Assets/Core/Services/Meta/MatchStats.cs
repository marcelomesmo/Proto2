namespace Core.Services.Meta
{
    public abstract class MatchStats
    {
        // Called when a match starts.
        public virtual void OnMatchStart() { }

        // Called when a match ends (victory or defeat).
        public virtual void OnMatchEnd() { }
        
        public virtual void OnMatchTimeUpdated(float time) { }
    }
}
