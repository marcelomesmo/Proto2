using System;
using Core.EventChannels;
using Core.EventChannels.Payloads;
using Core.Gameplay.Entity.Subsystem;
using Game.Entity.Player.Progression;
using Game.Entity.Player.Stats;
using UnityEngine;

namespace Game.Entity.Player.Subsystem
{
    public sealed class CharacterLevelSubsystem : BaseSubsystem
    {
        [Header("Config")]
        [SerializeField] private CharacterLevelUpData levelUpData;

        [Header("Broadcast")]
        [SerializeField] private CharacterLevelChangedEventChannelSO levelChangedEvent;
        [SerializeField] private CharacterLevelUpEventChannelSO levelUpEvent;
        
        [Header("Listener")]
        [SerializeField] private IntEventChannelSO xpEvent;

        private int _currentXp;
        private int _currentLevel;

        private CharacterStats _stats;
        private EntityPresentationSubsystem _presentation;

        public int Level => _currentLevel;
        public int XP => _currentXp;

        public event Action<int> OnLevelUp;

        protected override void OnInitialize()
        {
            _stats = Controller.Stats as CharacterStats;
            if (!_stats)
            {
                Debug.LogError(
                    $"[CharacterLevelSubsystem] Requires CharacterStats on {Controller.name}",
                    this);
                enabled = false;
                return;
            }
            
            _presentation = Controller.GetComponent<EntityPresentationSubsystem>();

            _currentXp = 0;
            _currentLevel = 0;

            xpEvent.OnEventRaised += HandleXpAwarded;
        }

        protected override void OnDeinitialize()
        {
            _presentation = null;
            
            xpEvent.OnEventRaised -= HandleXpAwarded;
        }
        
        private void HandleXpAwarded(int amount)
        {
            AddXp(amount);
        }

        // --------------------------------------------------
        // XP Intake
        // --------------------------------------------------

        public void AddXp(int amount)
        {
            if (!Controller || Controller.IsDead || Controller.IsReleased)
                return;
            
            if (amount <= 0 || _currentLevel >= levelUpData.MaxLevel)
                return;

            _currentXp += amount;

            TryLevelUp();
            
            levelChangedEvent?.RaiseEvent(
                new CharacterLevelChangedPayload(
                    _stats.characterId,
                    _currentLevel,
                    _currentXp
                ));

            //Debug.Log("Added " + amount + " XP to " + _stats.characterId + " (" + _currentLevel + "/" + progressionData.MaxLevel + ")");
        }

        private void TryLevelUp()
        {
            while (_currentLevel < levelUpData.MaxLevel)
            {
                int requiredXp =
                    levelUpData.GetXpForLevel(_currentLevel + 1);

                if (_currentXp < requiredXp)
                    break;

                PerformLevelUp();
            }
        }

        private void PerformLevelUp()
        {
            int oldLevel = _currentLevel;
            _currentLevel++;

            ApplyStatGain(_currentLevel);
            
            levelUpEvent?.RaiseEvent(
                new CharacterLevelUpPayload(
                    _stats.characterId,
                    oldLevel,
                    _currentLevel
                ));
            
            OnLevelUp?.Invoke(_currentLevel);
            
            //Debug.Log("Level up! " + _stats.characterId + " from " + oldLevel + " to " + _currentLevel);
        }

        private void ApplyStatGain(int level)
        {
            var gain = levelUpData.GetStatGainForLevel(level);

            _stats.attackPower += gain.attackPower;
            _stats.defense += gain.defense;
        }
    }
}