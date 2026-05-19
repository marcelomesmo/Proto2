using Game.Entity.Player.Progression;
using UnityEngine;

namespace Game.UI.CharacterHUD
{
    // Pure view helper.
    public sealed class CharacterPortraitResolver : MonoBehaviour
    {
        [SerializeField] private CharacterEvolutionData _evolutionData;
        [SerializeField] private Sprite _basePortrait;
        private int _currentStage;

        // Called once during slot initialization
        public void Initialize(
            CharacterEvolutionData evolutionData,
            Sprite basePortrait)
        {
            _evolutionData = evolutionData;
            _basePortrait = basePortrait;
        }
        
        public void SetStage(int stage)
        {
            _currentStage = stage;
        }

        public Sprite Resolve()
        {
            if (_evolutionData  &&
                _evolutionData.TryGetStage(_currentStage, out var stage) &&
                stage.portraitOverride)
            {
                return stage.portraitOverride;
            }

            return null; // Should never happen if data is valid
        }
    }
}