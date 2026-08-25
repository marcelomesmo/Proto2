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
        public CharacterEvolutionData EvolutionData => evolutionData;

        protected override void OnInitialize()
        {
            _levelSubsystem = GetComponent<CharacterLevelSubsystem>();
            
            // Force baseline stage for pooled reuse
            _currentStage = 0;
            if (evolutionData.TryGetStage(0, out var stage0))
                ApplyStage(stage0);

            _levelSubsystem.OnLevelUp += HandleLevelUp;

            // Now apply any evolution based on the current level (spawn at higher level case)
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
                loadout.InitializeFromDefinition(stage.attackLoadoutOverride);
            }
            
            // 2. Presentation changes
            if (Controller.TryGetComponent(out EntityPresentationSubsystem presentation))
            {
                presentation.ApplySpriteOverride(stage.spriteOverride);
                presentation.ApplyAnimatorOverride(stage.animatorOverride);
            }
        }

        private string GetCharacterId()
        {
            return Controller.Stats.characterId;
        }
        
        public void RestoreForLevel(int level)
        {
            if (evolutionData == null)
                return;

            int targetStage = evolutionData.GetStageForLevel(level);

            if (!evolutionData.TryGetStage(targetStage, out CharacterEvolutionData.EvolutionStage stageData))
            {
                Debug.LogError($"[CharacterEvolutionSubsystem] Missing stage {targetStage} data.", this);
                return;
            }

            _currentStage = targetStage;

            ApplyStage(stageData);
            
            // This is a silent method to restore state on new session, so we do not call PerformStageUp or gameplay feedback here.
        }
    }
}