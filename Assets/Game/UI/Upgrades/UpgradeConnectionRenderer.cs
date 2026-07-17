using System.Collections.Generic;
using Core.Enum;
using Core.Services;
using Core.Services.Meta;
using Core.Upgrades;
using Game.Enum;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.UI.Upgrades
{
    [ExecuteAlways]
    public sealed class UpgradeConnectionRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform connectionPrefab;

        [Tooltip("Root object that contains the UpgradeNode objects. If null, uses this object's parent.")]
        [SerializeField] private RectTransform nodesRoot;

        [Header("Editor Preview")]
        [SerializeField] private bool renderInEditor = true;

        [Header("Positioning")]
        [SerializeField] private float extraPadding = 0.25f;

        private const string GeneratedConnectionName = "__GeneratedUpgradeConnection";

        private readonly List<RectTransform> _connections = new();
        
        private readonly List<ConnectionBinding> _connectionBindings = new();

        private struct ConnectionBinding
        {
            public UpgradeConnectionView View;
            public UpgradeNode ParentNode;
            public UpgradeNode ChildNode;
        }

#if UNITY_EDITOR
        private bool _editorRebuildQueued;
#endif

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                QueueEditorRebuild();
#endif
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                QueueEditorRebuild();
#endif
        }

        private void OnTransformChildrenChanged()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                QueueEditorRebuild();
#endif
        }

#if UNITY_EDITOR
        [ContextMenu("Rebuild Connections")]
        private void RebuildFromSceneNodes()
        {
            if (Application.isPlaying)
                return;

            if (!renderInEditor)
            {
                Clear();
                return;
            }

            RectTransform root =
                nodesRoot != null
                    ? nodesRoot
                    : transform.parent as RectTransform;

            if (root == null)
                return;

            var nodes =
                new List<UpgradeNode>(
                    root.GetComponentsInChildren<UpgradeNode>(true));

            Build(nodes);

            EditorUtility.SetDirty(this);
        }

        private void QueueEditorRebuild()
        {
            if (_editorRebuildQueued)
                return;

            _editorRebuildQueued = true;

            EditorApplication.delayCall += () =>
            {
                if (this == null)
                    return;

                _editorRebuildQueued = false;

                if (Application.isPlaying)
                    return;

                RebuildFromSceneNodes();
            };
        }
#endif

        public void Build(List<UpgradeNode> nodes)
        {
            Clear();

            if (connectionPrefab == null || nodes == null)
                return;

            var lookup =
                new Dictionary<UpgradeDefinition, UpgradeNode>();

            foreach (var node in nodes)
            {
                if (node == null)
                    continue;

                if (node.Definition != null)
                    lookup[node.Definition] = node;
            }

            foreach (var node in nodes)
            {
                if (node == null)
                    continue;

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
                        parentNode,
                        node);
                }
            }
            
            RefreshConnectionStates();
        }
        
        public void RefreshConnectionStates()
        {
            foreach (var binding in _connectionBindings)
            {
                if (binding.View == null)
                    continue;
                
#if UNITY_EDITOR
                // Keep the complete tree visible while editing the layout.
                if (!Application.isPlaying)
                {
                    binding.View.gameObject.SetActive(true);
                    binding.View.SetState(UpgradeConnectionState.Inactive);
                    continue;
                }
#endif
                bool shouldBeVisible =
                    binding.ParentNode != null &&
                    binding.ChildNode != null &&
                    binding.ParentNode.IsRevealed &&
                    binding.ChildNode.IsRevealed;

                if (binding.View.gameObject.activeSelf != shouldBeVisible)
                    binding.View.gameObject.SetActive(shouldBeVisible);

                if (!shouldBeVisible)
                    continue;
                
                /*if (binding.ChildNode == null)
                {
                    binding.View.SetState(UpgradeConnectionState.Inactive);
                    continue;
                }*/

                UpgradeConnectionState state =
                    binding.ParentNode.IsPurchased &&
                    binding.ChildNode.IsPurchased
                        ? UpgradeConnectionState.Purchased
                        : UpgradeConnectionState.Available;

                binding.View.SetState(state);
                //binding.View.SetState(
                //    binding.ChildNode.GetIncomingConnectionState());
            }
        }

        private void CreateConnection(
            UpgradeNode fromNode,
            UpgradeNode toNode)
        {
            if (fromNode == null || toNode == null)
                return;

            RectTransform from =
                fromNode.transform as RectTransform;

            RectTransform to =
                toNode.transform as RectTransform;
            
            if (from == null || to == null)
                return;
            
            RectTransform connection =
                InstantiateConnection();

            connection.name = GeneratedConnectionName;

            Vector2 start =
                transform.InverseTransformPoint(from.position);

            Vector2 end =
                transform.InverseTransformPoint(to.position);

            Vector2 direction = end - start;

            if (direction.sqrMagnitude > 0.001f)
            {
                Vector2 normalized = direction.normalized;

                float nodeSizePadding =
                    Mathf.Min(
                        from.rect.width,
                        from.rect.height) * 0.5f;

                start += normalized * nodeSizePadding * extraPadding;
                end -= normalized * nodeSizePadding * extraPadding;
            }

            direction = end - start;

            float length = direction.magnitude;
            
            connection.pivot = new Vector2(0f, 0.5f);
            connection.anchoredPosition = start;
            
            UpgradeConnectionView view = null;

            if (connection.TryGetComponent(out view))
            {
                view.SetLength(length);
            }
            else
            {
                connection.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    length);
            }

            if (view != null)
            {
                _connectionBindings.Add(
                    new ConnectionBinding
                    {
                        View = view,
                        ParentNode = fromNode,
                        ChildNode = toNode
                    });

                //view.SetState(
                //    toNode.GetIncomingConnectionState());
            }
            
            //connection.sizeDelta =
            //    new Vector2(
            //        length,
            //        connection.sizeDelta.y);

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

        private RectTransform InstantiateConnection()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                var prefabInstance =
                    PrefabUtility.InstantiatePrefab(
                        connectionPrefab,
                        transform);

                if (prefabInstance is RectTransform rectTransform)
                    return rectTransform;

                return Instantiate(connectionPrefab, transform);
            }
#endif

            return Instantiate(connectionPrefab, transform);
        }

        private void Clear()
        {
            _connections.Clear();
            _connectionBindings.Clear();
            
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);

                if (!child.name.StartsWith(GeneratedConnectionName))
                    continue;

                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }
        }
    }
}