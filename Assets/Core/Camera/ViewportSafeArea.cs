using UnityEngine;

namespace Core.Camera
{
    [ExecuteAlways]
    public class ViewportSafeArea : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera targetCamera;

        private RectTransform _rect;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        private void FixedUpdate()
        {
            Debug.DrawLine(
                targetCamera.pixelRect.min,
                targetCamera.pixelRect.max,
                Color.magenta
            );
        }

        private void LateUpdate()
        {
            if (!targetCamera)
                return;

            ApplyCameraViewport();
        }

        private void ApplyCameraViewport()
        {
            // Camera pixel rect (actual rendered area)
            Rect pixelRect = targetCamera.pixelRect;

            // Convert screen pixels → normalized canvas space
            float screenW = Screen.width;
            float screenH = Screen.height;

            Vector2 anchorMin = new Vector2(
                pixelRect.xMin / screenW,
                pixelRect.yMin / screenH
            );

            Vector2 anchorMax = new Vector2(
                pixelRect.xMax / screenW,
                pixelRect.yMax / screenH
            );

            _rect.anchorMin = anchorMin;
            _rect.anchorMax = anchorMax;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}