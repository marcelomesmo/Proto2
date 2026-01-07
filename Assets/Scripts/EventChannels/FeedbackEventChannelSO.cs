using System;
using UnityEngine;

namespace EventChannels
{
    [CreateAssetMenu(fileName = "FeedbackEventChannelSO", menuName = "Events/Feedback Event Channel")]
    public class FeedbackEventChannelSO : ScriptableObject
    {
        public Action<FeedbackRequest> OnEventRaised;

        public void RaiseEvent(FeedbackRequest request)
        {
            OnEventRaised?.Invoke(request);
        }
    }
}
