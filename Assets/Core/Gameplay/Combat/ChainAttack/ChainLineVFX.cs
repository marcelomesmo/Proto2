using UnityEngine;

namespace Core.Gameplay.Combat.ChainAttack
{
    [RequireComponent(typeof(LineRenderer))]
    public class ChainLineVfx : MonoBehaviour, IChainVfx
    {
        private Transform _from;
        private Transform _to;
        private LineRenderer _line;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.useWorldSpace = true;
            _line.positionCount = 2;
        }

        public void Initialize(Transform from, Transform to)
        {
            _from = from;
            _to = to;
        }

        private void LateUpdate()
        {
            if (!_from || !_to)
            {
                Destroy(gameObject);
                return;
            }

            _line.SetPosition(0, _from.position);
            _line.SetPosition(1, _to.position);
        }
    }
}
