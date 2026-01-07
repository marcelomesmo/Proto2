using EventChannels;
using UnityEngine;

namespace Entity.Effects
{
    public class PlayerLightsSubsystem : BaseSubsystem
    {
        [Header("References")]
        [SerializeField] private Transform frontalLight;
        
        [Header("Broadcast Receiver")]
        [SerializeField] private FeedbackEventChannelSO directionChangedEvent;
        
        // The rotation that represents "facing right" in your prefab
        private float _baseZ;
        
        void Awake()
        {
            _baseZ = frontalLight.localEulerAngles.z;   // Cache the initial rotation from the prefab automatically
        }
        
        private void OnEnable()
        {
            directionChangedEvent.OnEventRaised += HandleDirectionChanged;
        }

        private void HandleDirectionChanged(FeedbackRequest obj)
        {
            // direction: -1 = left, +1 = right
            float targetZ = obj.Intensity == 1
                ? _baseZ
                : _baseZ + 180f;
            
            frontalLight.localRotation = Quaternion.Euler(0f, 0f, targetZ);
        }
    }
}
