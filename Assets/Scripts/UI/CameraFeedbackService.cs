using EventChannels;
using Unity.Cinemachine;
using UnityEngine;

namespace UI
{
    public class CameraFeedbackService : MonoBehaviour
    {
        [Header("Cinemachine")]
        [SerializeField] private CinemachineImpulseSource impulseSource;

        [Header("Shake Presets")]
        [SerializeField] private ShakePreset smallShake;
        [SerializeField] private ShakePreset mediumShake;
        [SerializeField] private ShakePreset largeShake;
        
        public void Handle(FeedbackRequest request)
        {
            switch (request.Type)
            {
                case FeedbackType.CameraShake_Small:
                    PlayShake(smallShake, request.Intensity);
                    break;

                case FeedbackType.CameraShake_Medium:
                    PlayShake(mediumShake, request.Intensity);
                    break;

                case FeedbackType.CameraShake_Large:
                    PlayShake(largeShake, request.Intensity);
                    break;

                /*
                 e.g.
                case FeedbackType.Explosion:
                case FeedbackType.HeavyHit:
                case FeedbackType.CriticalHit:
                    PlayShake(mediumShake, request.Intensity);
                    break;
                */
                case FeedbackType.Land:
                    Debug.Log("Playing smallShake on Land with intensity " + request.Intensity);
                    PlayShake(smallShake, request.Intensity);
                    break;
                
                case FeedbackType.ExtractionStart:
                    PlayShake(largeShake, request.Intensity);
                    break;
            }
        }

        private void PlayShake(ShakePreset preset, float intensity)
        {
            if (!impulseSource || !preset)
                return;
            
            impulseSource.ImpulseDefinition.AmplitudeGain = preset.amplitude;
            impulseSource.ImpulseDefinition.FrequencyGain = preset.frequency;

            Vector3 force = preset.velocity * intensity;
            impulseSource.GenerateImpulse(force);
        }
    }
}