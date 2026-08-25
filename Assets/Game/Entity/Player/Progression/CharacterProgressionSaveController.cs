using Core.EventChannels;
using Core.EventChannels.Payloads;
using Core.Services;
using Core.Services.Save;
using Game.Services.Save;
using UnityEngine;

namespace Game.Entity.Player.Progression
{
    //
    // Persists runtime Character XP changes.
    //
    // CharacterLevelSubsystem reports progression through the
    // event channel and remains unaware of persistence.
    //
    public sealed class CharacterProgressionSaveController : MonoBehaviour
    {
        [SerializeField] private CharacterLevelChangedEventChannelSO levelChangedEvent;

        private GameSaveManager _saveManager;

        private void OnEnable()
        {
            _saveManager = ServiceLocator.Get<ISaveManager>() as GameSaveManager;

            if (_saveManager == null)
            {
                Debug.LogError("[CharacterProgressionSaveController] GameSaveManager service not found.", this);
                return;
            }

            if (levelChangedEvent == null)
            {
                Debug.LogError("[CharacterProgressionSaveController] CharacterLevelChangedEventChannelSO is not assigned.", this);
                return;
            }

            levelChangedEvent.OnEventRaised += HandleLevelChanged;
        }

        private void OnDisable()
        {
            if (levelChangedEvent != null)
            {
                levelChangedEvent.OnEventRaised -= HandleLevelChanged;
            }
        }

        private void HandleLevelChanged(CharacterLevelChangedPayload payload)
        {
            if (_saveManager == null)
                return;

            _saveManager.SetCharacterXp(payload.characterId, payload.xp);
        }
    }
}