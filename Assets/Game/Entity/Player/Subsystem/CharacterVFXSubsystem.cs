using Core.EventChannels;
using Core.EventChannels.Payloads;
using Core.Gameplay.Entity.Subsystem;
using UnityEngine;

namespace Game.Entity.Player.Subsystem
{
    public class CharacterVFXSubsystem : EntityVFXSubsystem
    {
        [Header("Player Character")]
        [SerializeField] private GameObject levelUpVfxPrefab;
        [SerializeField] private GameObject evolutionVfxPrefab;
        
        [Header("Listener")]
        [SerializeField] private CharacterLevelUpEventChannelSO levelUpEvent;
        [SerializeField] private CharacterStageUpEventChannelSO stageUpEvent;
    
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            levelUpEvent.OnEventRaised += PlayLevelUpVfx;
            stageUpEvent.OnEventRaised += PlayStageUpVfx;
        }
        
        protected override void OnDeinitialize()
        {
            base.OnDeinitialize();
            
            levelUpEvent.OnEventRaised -= PlayLevelUpVfx;
            stageUpEvent.OnEventRaised -= PlayStageUpVfx;
        }
        
        private void PlayLevelUpVfx(CharacterLevelUpPayload payload)
        {
            if (!levelUpVfxPrefab)
                return;

            if (!MatchesThisCharacter(payload.characterId))
                return;
            
            // todo: spawn vfx pooled
            //Spawn(levelUpVfxPrefab, this.transform, true);
        
            var vfx = Instantiate(
                levelUpVfxPrefab,
                transform.position,             // No specific anchor yet
                Quaternion.identity);

            Destroy(vfx, 2f);
        }
        
        private void PlayStageUpVfx(CharacterStageUpPayload payload)
        {
            if (!evolutionVfxPrefab)
                return;

            if (!MatchesThisCharacter(payload.characterId))
                return;
            
            // todo: spawn vfx pooled
            //Spawn(evolutionVfxPrefab, this.transform, true);

            var vfx = Instantiate(
                evolutionVfxPrefab,
                transform.position,
                Quaternion.identity);

            Destroy(vfx, 3f);
        }

        private bool MatchesThisCharacter(string id)
        {
            if (Controller.Stats is Stats.CharacterStats cs)
                return cs.characterId == id;

            return false;
        }
    }
}
