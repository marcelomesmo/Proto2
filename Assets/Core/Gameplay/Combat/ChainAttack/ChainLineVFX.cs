using UnityEngine;

namespace Core.Gameplay.Combat.ChainAttack
{
    [RequireComponent(typeof(LineRenderer))]
    public class ChainLineVfx : MonoBehaviour, IChainVfx
    {
         [Header("Pixel Settings")]
        [SerializeField] private bool snapToPixelGrid = true;
        [SerializeField] private float pixelsPerUnit = 32f;
        [SerializeField] private float widthInPixels = 1f;

        [Header("Shape")]
        [SerializeField] private bool useSteppedShape = false;
        [SerializeField] private float stepOffsetInPixels = 1f;

        [Header("Lifetime")]
        [SerializeField] private bool destroyWhenTargetIsMissing = true;

        private Transform _from;
        private Transform _to;
        private LineRenderer _line;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();

            _line.useWorldSpace = true;
            _line.positionCount = useSteppedShape ? 4 : 2;

            // Pixel-art-friendly defaults.
            _line.numCapVertices = 0;
            _line.numCornerVertices = 0;
            _line.textureMode = LineTextureMode.Tile;

            float width = widthInPixels / pixelsPerUnit;
            _line.startWidth = width;
            _line.endWidth = width;
        }

        public void Initialize(Transform from, Transform to)
        {
            _from = from;
            _to = to;

            UpdateLine();
        }

        private void LateUpdate()
        {
            if (!_from || !_to)
            {
                if (destroyWhenTargetIsMissing)
                    Destroy(gameObject);

                return;
            }

            UpdateLine();
        }

        private void UpdateLine()
        {
            Vector3 fromPosition = _from.position;
            Vector3 toPosition = _to.position;

            if (snapToPixelGrid)
            {
                fromPosition = SnapToPixelGrid(fromPosition);
                toPosition = SnapToPixelGrid(toPosition);
            }

            if (useSteppedShape)
            {
                SetSteppedLine(fromPosition, toPosition);
            }
            else
            {
                SetStraightLine(fromPosition, toPosition);
            }
        }

        private void SetStraightLine(Vector3 fromPosition, Vector3 toPosition)
        {
            if (_line.positionCount != 2)
                _line.positionCount = 2;

            _line.SetPosition(0, fromPosition);
            _line.SetPosition(1, toPosition);
        }

        private void SetSteppedLine(Vector3 fromPosition, Vector3 toPosition)
        {
            if (_line.positionCount != 4)
                _line.positionCount = 4;

            Vector3 direction = toPosition - fromPosition;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f).normalized;

            float stepOffset = stepOffsetInPixels / pixelsPerUnit;

            Vector3 pointA = Vector3.Lerp(fromPosition, toPosition, 0.33f);
            Vector3 pointB = Vector3.Lerp(fromPosition, toPosition, 0.66f);

            pointA += perpendicular * stepOffset;
            pointB -= perpendicular * stepOffset;

            if (snapToPixelGrid)
            {
                pointA = SnapToPixelGrid(pointA);
                pointB = SnapToPixelGrid(pointB);
            }

            _line.SetPosition(0, fromPosition);
            _line.SetPosition(1, pointA);
            _line.SetPosition(2, pointB);
            _line.SetPosition(3, toPosition);
        }

        private Vector3 SnapToPixelGrid(Vector3 position)
        {
            float unit = 1f / pixelsPerUnit;

            position.x = Mathf.Round(position.x / unit) * unit;
            position.y = Mathf.Round(position.y / unit) * unit;

            return position;
        }
    }
}
