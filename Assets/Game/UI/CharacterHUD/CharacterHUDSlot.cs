using Core.EventChannels;
using Core.EventChannels.Payloads;
using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Attack;
using Core.Gameplay.Entity.Subsystem;
using Game.Entity.Player.Progression;
using Game.Entity.Player.Subsystem;
using UnityEngine;

namespace Game.UI.CharacterHUD
{
    public sealed class CharacterHUDSlot : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private CharacterHUDSlotView view;
        [SerializeField] private CharacterHUDAttacksView attacksView;

        [Header("Events")]
        [SerializeField] private CharacterLevelChangedEventChannelSO levelChangedEvent;
        [SerializeField] private CharacterStageChangedEventChannelSO stageChangedEvent;
        
        private EntityController _character;
        private string _characterId;

        public void Initialize(EntityController character, CharacterEvolutionData evolutionData)
        {
            _character = character;
            _characterId = character.Stats.characterId;
            
            // --- CharacterPortraitResolver setup ---
            if (view.TryGetComponent(out CharacterPortraitResolver resolver))
            {
                resolver.Initialize(
                    evolutionData,
                    basePortrait: evolutionData.stages[0].portraitOverride  // Stage 0 portrait
                    //basePortrait: character.Stats.portrait   <- it isn't in Stats but in definition originally
                    // todo: unify the stage 0 basePortrait in either evolutionData or Stats for the roster screen as well.
                );
            }
            
            BindInitialPresentation(evolutionData);
            BindAttacks();
        }

        private void OnEnable()
        {
            levelChangedEvent.OnEventRaised += HandleLevelChanged;
            stageChangedEvent.OnEventRaised += HandleStageChanged;
        }

        private void OnDisable()
        {
            levelChangedEvent.OnEventRaised -= HandleLevelChanged;
            stageChangedEvent.OnEventRaised -= HandleStageChanged;
        }
        
        // --------------------------------------------------
        // Bindings
        // --------------------------------------------------

        private void BindAttacks()
        {
            if (!_character.TryGetComponent(out EntityAttackSubsystem attacks))
                return;

            if (!_character.TryGetComponent(out EntityAttackLoadout loadout))
                return;
            
            attacksView.BindAttacks(attacks, loadout.Attacks);
        }

        private void BindInitialPresentation(CharacterEvolutionData evolutionData)
        {
            // Portrait
            if (_character.TryGetComponent(out CharacterEvolutionSubsystem evolution))
            {
                if (evolutionData.TryGetStage(evolution.Stage, out var stage))
                    view.SetPortrait(stage.portraitOverride);
            }

            // Level
            if (_character.TryGetComponent(out CharacterLevelSubsystem level))
                view.SetLevel(level.Level);
        }

        // --------------------------------------------------
        // Events
        // --------------------------------------------------
        
        private void HandleLevelChanged(CharacterLevelChangedPayload payload)
        {
            if (payload.characterId != _characterId)
                return;

            view.SetLevel(payload.level);
        }

        private void HandleStageChanged(CharacterStageChangedPayload payload)
        {
            if (payload.characterId != _characterId)
                return;

            // Portrait + attacks change on evolution
            view.SetStage(payload.stage);
            BindAttacks();
        }
    }
}