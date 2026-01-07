using System;
using Core.Enum;
using UnityEngine;

namespace Core.EventChannels
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
