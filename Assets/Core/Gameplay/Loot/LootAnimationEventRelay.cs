using UnityEngine;

namespace Core.Gameplay.Loot
{
    public class LootAnimationEventRelay : MonoBehaviour
    {
        // Reference to the script on the parent GameObject that needs to receive the event
        public BaseLoot parentScript;

        // This method will be called by the Animation Event
        public void OnAnimationFinished()
        {
            if (parentScript)
                parentScript.FinishCollect();
            else
                Debug.LogWarning("[LootAnimationEventRelay] ParentScript reference not set on CollectableAnimationEventRelay!");
        }
    }
}