using Core.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Upgrades
{
    public sealed class UpgradeTooltip : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI description;
        [SerializeField] private TextMeshProUGUI cost;
        
        [Header("Positioning")]
        [SerializeField] private Vector2 offset = new(0, 20);

        private RectTransform _rect;
        private RectTransform _parentRect;
        private RectTransform _boundsRect;
        
        private void Awake()
        {
            _rect = transform as RectTransform;
            _parentRect = transform.parent as RectTransform;
            _boundsRect = transform.parent.parent as RectTransform;
        }
        
        public void Show(UpgradeDefinition def, int currentLevel, RectTransform nodePosition)
        {
            if (def == null || nodePosition == null)
                return;
            
            gameObject.SetActive(true);

            title.text = def.displayName;
            description.text = def.GetEffectDescriptionForNextLevel(currentLevel);

            if (currentLevel < def.maxLevel)
                cost.text = $"Cost: {TMPIcons.Currency} {def.GetCostForLevel(currentLevel + 1)}";
            else
                cost.text = "Maxed";
            
            // Force update to adapt to size
            title.ForceMeshUpdate();
            description.ForceMeshUpdate();
            cost.ForceMeshUpdate();

            Canvas.ForceUpdateCanvases();
            
            // Rebuild after text changes to capture tooltip size properly. Specially useful if later we make dynamic sizes.
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);

            // In the future, if we add zoom/pan, we need to position the tooltip to the tree
            //  camera/content coordinate space instead.
            //PositionAt(nodePosition);
            
            PositionAtAdaptative(nodePosition);
        }
        
        public void Refresh(UpgradeDefinition def, int currentLevel, RectTransform nodePosition)
        {
            Show(def, currentLevel, nodePosition);

            // In the future, we can add other UI changes here:
            // like a VFX, or changing the borders per level, or other visual feedbacks.
            // eg. def.IsMaxLevel -> spawn(vfx)
        }

        private void PositionAt(RectTransform nodePosition)
        {
            Vector3[] corners = new Vector3[4];
            nodePosition.GetWorldCorners(corners);

            // top center of node
            //Vector3 worldPosition =
            //    (corners[1] + corners[2]) * 0.5f;
            
            // top right of node
            Vector3 worldPosition = corners[2];

            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRect,
                worldPosition,
                null,
                out localPoint);
            
            // Make sure layout has calculated the tooltip size
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);
            
            float tooltipHalfHeight =
                _rect.rect.height * 0.5f;

            //_rect.localPosition =
            //    localPoint + offset;
            
            _rect.localPosition =
                localPoint
                + new Vector2(
                    0,
                    tooltipHalfHeight)
                + offset;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        private void PositionAtAdaptative(RectTransform node)
        {
            Vector3[] corners = new Vector3[4];
            node.GetWorldCorners(corners);

            Vector3 nodeLeftWorld = corners[1];  // top-left
            Vector3 nodeRightWorld = corners[2]; // top-right

            Vector2 nodeRightLocal;
            Vector2 nodeLeftLocal;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _boundsRect,
                RectTransformUtility.WorldToScreenPoint(null, nodeRightWorld),
                null,
                out nodeRightLocal);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _boundsRect,
                RectTransformUtility.WorldToScreenPoint(null, nodeLeftWorld),
                null,
                out nodeLeftLocal);

            Rect bounds = _boundsRect.rect;

            float tooltipWidth = _rect.rect.width;
            float tooltipHeight = _rect.rect.height;

            bool fitsRight =
                nodeRightLocal.x + tooltipWidth + offset.x <= bounds.xMax;

            bool fitsAbove =
                nodeRightLocal.y + tooltipHeight + offset.y <= bounds.yMax;

            Vector2 targetLocalInBounds = fitsRight
                ? nodeRightLocal
                : nodeLeftLocal;

            targetLocalInBounds.x += fitsRight
                ? tooltipWidth * 0.5f + offset.x
                : -tooltipWidth * 0.5f - offset.x;

            targetLocalInBounds.y += fitsAbove
                ? tooltipHeight * 0.5f + offset.y
                : -tooltipHeight * 0.5f - offset.y;

            Vector3 targetWorld =
                _boundsRect.TransformPoint(targetLocalInBounds);

            Vector2 targetLocalInParent;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRect,
                RectTransformUtility.WorldToScreenPoint(null, targetWorld),
                null,
                out targetLocalInParent);

            _rect.anchoredPosition = targetLocalInParent;
        }
        
        private void PositionAtAdaptativeScreen(RectTransform node)
        {
            Vector3[] corners = new Vector3[4];
            node.GetWorldCorners(corners);

            // Node edges
            Vector3 nodeRight = corners[2];
            Vector3 nodeLeft = corners[1];
            Vector3 nodeTop = corners[1];

            Vector2 nodeScreen =
                RectTransformUtility.WorldToScreenPoint(
                    null,
                    nodeRight);

            bool fitsRight =
                nodeScreen.x + _rect.rect.width * 0.5f + offset.x < Screen.width;

            Vector3 targetWorld;
            
            if (fitsRight)
            {
                // Tooltip center goes to the right of node
                targetWorld = nodeRight;
            }
            else
            {
                // Tooltip center goes to the left of node
                targetWorld = nodeLeft;
            }

            Vector2 localPoint;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentRect,
                RectTransformUtility.WorldToScreenPoint(
                    null,
                    targetWorld),
                null,
                out localPoint);

            // Move tooltip above/below node
            bool fitsAbove =
                nodeScreen.y + _rect.rect.height < Screen.height;

            localPoint.y += fitsAbove
                ? _rect.rect.height * 0.5f + offset.y
                : -_rect.rect.height * 0.5f - offset.y;

            localPoint.x += fitsRight
                ? _rect.rect.width * 0.5f + offset.x
                : -_rect.rect.width * 0.5f - offset.x;

            _rect.localPosition = localPoint;
        }
    }
}
