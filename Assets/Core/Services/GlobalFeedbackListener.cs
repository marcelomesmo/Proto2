using Core.Camera;
using Core.Enum;
using Core.EventChannels;
using UnityEngine;

namespace Core.Services
{
    /*
        Responsible to:
        - Listen to feedback events
        - Route them to services
        - Filter by scope
     */
    public class GlobalFeedbackListener : MonoBehaviour
    {
        [SerializeField] private FeedbackEventChannelSO feedbackEvent;

        [SerializeField] private CameraFeedbackService cameraService;
        //[SerializeField] VFXFeedbackService vfx;
        //[SerializeField] AudioFeedbackService audio;
        
        private void OnEnable()
        {
            feedbackEvent.OnEventRaised += OnFeedbackRequested;
        }

        private void OnDisable()
        {
            feedbackEvent.OnEventRaised -= OnFeedbackRequested;
        }
        
        private void OnFeedbackRequested(FeedbackRequest request)
        {
            if (request.Scope != FeedbackScope.Global)
                return;

            //Debug.Log("Passing request from global listener to Camera service.");
            cameraService?.Handle(request);
            //vfx?.Handle(request);
            //audio?.Handle(request);
        }
    }
}
