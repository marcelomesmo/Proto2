using System.Collections.Generic;
using Core.Upgrades;
using UnityEngine;

namespace Game.UI.Upgrades
{
    public sealed class UpgradeConnectionRenderer : MonoBehaviour
    {
        [SerializeField] private RectTransform connectionPrefab;
        private readonly List<RectTransform> _connections = new();
        
        public void Build(List<UpgradeNode> nodes)
        {
            Clear();

            var lookup = 
                new Dictionary<UpgradeDefinition, UpgradeNode>();

            foreach (var node in nodes)
            {
                if (node.Definition != null)
                    lookup[node.Definition] = node;
            }

            foreach (var node in nodes)
            {
                var definition = node.Definition;

                if (definition == null)
                    continue;

                var rule = definition.UnlockRule;

                if (rule == null)
                    continue;

                foreach (var requirement in rule.Requirements)
                {
                    if (requirement?.Upgrade == null)
                        continue;

                    if (!lookup.TryGetValue(
                            requirement.Upgrade,
                            out var parentNode))
                    {
                        continue;
                    }

                    CreateConnection(
                        parentNode.GetComponent<RectTransform>(),
                        node.GetComponent<RectTransform>());
                }
            }
        }

        private void CreateConnection(
            RectTransform from,
            RectTransform to)
        {
            var connection =
                Instantiate(
                    connectionPrefab,
                    transform);
            
            // In case they are in the same hierarchy:
            //Vector2 start = from.anchoredPosition;
            //Vector2 end = to.anchoredPosition;
            
            // In case they are not:
            Vector2 start =
                transform.InverseTransformPoint(
                    from.position);

            Vector2 end =
                transform.InverseTransformPoint(
                    to.position);

            Vector2 direction = end - start;
            
            if (direction.sqrMagnitude > 0.001f)
            {
                Vector2 normalized = direction.normalized;
                
                float nodeSizePadding =
                    Mathf.Min(
                        from.rect.width,
                        from.rect.height) * 0.5f;

                const float extraPadding = 1.25f;

                start += normalized * nodeSizePadding * extraPadding;
                end -= normalized * nodeSizePadding * extraPadding;
            }
            
            direction = end - start;

            float length = direction.magnitude;

            connection.anchoredPosition = start;

            connection.sizeDelta =
                new Vector2(
                    length,
                    connection.sizeDelta.y);

            float angle =
                Mathf.Atan2(
                    direction.y,
                    direction.x)
                * Mathf.Rad2Deg;

            connection.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle);

            _connections.Add(connection);
        }

        private void Clear()
        {
            foreach (var connection in _connections)
            {
                if (connection != null)
                    Destroy(connection.gameObject);
            }

            _connections.Clear();
        }
    }
}
