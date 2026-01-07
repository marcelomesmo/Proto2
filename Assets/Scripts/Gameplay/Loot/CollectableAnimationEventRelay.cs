using Gameplay.Loot;
using UnityEngine;

public class CollectableAnimationEventRelay : MonoBehaviour
{
    // Reference to the script on the parent GameObject that needs to receive the event
    public BaseCollectable parentScript;

    // This method will be called by the Animation Event
    public void OnAnimationFinished()
    {
        if (parentScript)
            parentScript.FinishCollect();
        else
            Debug.LogWarning("ParentScript reference not set on CollectableAnimationEventRelay!");
    }
}