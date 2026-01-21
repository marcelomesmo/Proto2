namespace Core.Services.Meta
{
    public sealed class NullMatchStats : MatchStats
    {
        public override void OnMatchStart() { }
        public override void OnMatchEnd() { }
        public override void OnMatchTimeUpdated(float time) { }
    }
}