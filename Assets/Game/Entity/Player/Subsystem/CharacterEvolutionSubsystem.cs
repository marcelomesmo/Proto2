using Core.EventChannels;
using Core.EventChannels.Payloads;
using Core.Gameplay.Entity.Attack;
using Core.Gameplay.Entity.Subsystem;
using Game.Entity.Player.Progression;
using UnityEngine;

namespace Game.Entity.Player.Subsystem
{
    [RequireComponent(typeof(CharacterLevelSubsystem))]
    public sealed class CharacterEvolutionSubsystem : BaseSubsystem
    {
        [Header("Config")]
        [SerializeField] private CharacterEvolutionData evolutionData;

        [Header("Broadcast")]
        [SerializeField] private CharacterStageChangedEventChannelSO stageChangedEvent;
        [SerializeField] private CharacterStageUpEventChannelSO stageUpEvent;

        private CharacterLevelSubsystem _levelSubsystem;
        private int _currentStage;

        public int Stage => _currentStage;

        protected override void OnInitialize()
        {
            _levelSubsystem = GetComponent<CharacterLevelSubsystem>();
            _currentStage = 0;

            _levelSubsystem.OnLevelUp += HandleLevelUp;

            // Ensure correct initial state (spawn at higher level case)
            EvaluateEvolution(_levelSubsystem.Level, force: true);
        }

        protected override void OnDeinitialize()
        {
            _levelSubsystem.OnLevelUp -= HandleLevelUp;
            _levelSubsystem = null;
        }

        private void HandleLevelUp(int newLevel)
        {
            EvaluateEvolution(newLevel, force: false);
        }

        private void EvaluateEvolution(int level, bool force)
        {
            int targetStage = evolutionData.GetStageForLevel(level);

            if (!force && targetStage <= _currentStage)
                return;

            while (_currentStage < targetStage)
            {
                PerformStageUp(_currentStage + 1);
            }
        }

        private void PerformStageUp(int newStage)
        {
            if (!evolutionData.TryGetStage(newStage, out var stageData))
            {
                Debug.LogError(
                    $"[CharacterEvolutionSubsystem] Missing stage {newStage} data on {Controller.name}",
                    this);
                return;
            }

            int oldStage = _currentStage;
            _currentStage = newStage;

            ApplyStage(stageData);

            stageUpEvent?.RaiseEvent(
                new CharacterStageUpPayload(
                    GetCharacterId(),
                    oldStage,
                    _currentStage
                ));

            stageChangedEvent?.RaiseEvent(
                new CharacterStageChangedPayload(
                    GetCharacterId(),
                    _currentStage
                ));
        }

        private void ApplyStage(CharacterEvolutionData.EvolutionStage stage)
        {
            // 1. Swap attack loadout
            if (stage.attackLoadoutOverride &&
                Controller.TryGetComponent(out EntityAttackLoadout loadout))
            {
                Debug.Log(Controller.name + " is changing loadout.");
                loadout.InitializeFromDefinition(stage.attackLoadoutOverride);
            }
            
            // 2. Presentation changes
            if (Controller.TryGetComponent(out EntityPresentationSubsystem presentation))
            {
                Debug.Log(Controller.name + " is changing sprite.");
                presentation.ApplySpriteOverride(stage.spriteOverride);
                Debug.Log(Controller.name + " is changing animator.");
                presentation.ApplyAnimatorOverride(stage.animatorOverride);
            }
        }

        private string GetCharacterId()
        {
            return Controller.Stats.characterId;
        }
    }
}