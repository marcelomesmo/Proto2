using UnityEngine;

namespace Core.Util
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class YSortRenderer : MonoBehaviour
    {
        [SerializeField] private int sortingOffset = 0;
        [SerializeField] private float precision = 100f;

        private SpriteRenderer _sr;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            _sr.sortingOrder = sortingOffset - Mathf.RoundToInt(transform.position.y * precision);
        }
    }
}
