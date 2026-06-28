using UnityEngine;

namespace Core.VFX
{
    // This needs to be attached to the exit state in your Animator Controllers.
    // Required for all PooledAnimatorVFX, otherwise they won't be released.
    public class VFXAutoRelease : StateMachineBehaviour
    {
        public override void OnStateEnter(
            Animator animator,
            AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            animator.GetComponent<PooledAnimatorVFX>()?.OnAnimationFinished();
        }
    }
}