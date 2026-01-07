using Core.Gameplay.Entity;
using Core.Gameplay.Entity.Tags;
using Core.Services;
using UnityEngine;

namespace Entity
{
    public class PlayerController : EntityController
    {
        protected override void Awake()
        {
            base.Awake();
            
            if(!Stats)
                throw new System.Exception("[PlayerController] Stats not set!");

            InitializeAllSubsystems();
        }
        
        protected override void HandleTagAdded(GameplayTag tag)
        {
            if (tag == Stats.deadTag)
                HandleDeath();
        }
        
        private void HandleDeath()
        {
            // Play VFX, handle stuff.
            
            Tags.ClearTemporaryTags();
        }

        public override void NotifyDeathAnimationFinished()
        {
            // HARD INVARIANT: only valid if already dead
            if (!IsDead)
                return;
            
            DeinitializeAllSubsystems();

            // TODO: This should only be cast after the end of level flow?
            // Tags.ClearTags();

            // TODO: Play the end of level flow.
            ServiceLocator.Get<GameController>().OnGameEnded();
            SceneLoader.LoadMenu();
        }
    }
}