using Core.Enum;

namespace Core.Services.Meta
{
    // Core-defined lifecycle extension point.
    // Implemented by the Game layer to inject match-specific orchestration
    // (e.g., loading runtime upgrades, difficulty, mutators) without Core->Game coupling.
    public interface IMatchLifecycleHandler
    {
        void OnMatchStart(GameController gameController);
        void HandleMatchEnded(GameController gameController, MatchEndReason reason);
        void ConfirmExit();
    }
}
