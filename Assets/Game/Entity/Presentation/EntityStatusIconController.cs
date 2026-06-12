using System;
using System.Collections.Generic;
using Core.Gameplay.Entity.Tags;
using UnityEngine;

namespace Game.Entity.Presentation
{
    public sealed class EntityStatusIconController : MonoBehaviour
    {
        [Serializable]
        private class StatusIconBinding
        {
            public GameplayTag tag;
            public GameObject icon; // rename to root when we have timers, etc (i.e. more than just icon)
        }
        
        [Header("Layout")]
        [SerializeField] private Transform container;
        [SerializeField] private float spacing = 0.05f;

        [Header("Status Icons")]
        [SerializeField] private List<StatusIconBinding> icons;

        private Dictionary<GameplayTag, GameObject> _lookup = new();
        private List<GameObject> _orderedIcons = new();

        private void Awake()
        {
            BuildLookup();
        }
        
        private void BuildLookup()
        {
            // Add all icons (pair tag:icon) to the lookup dictionary
            foreach(var entry in icons)
            {
                if(entry.tag == null || entry.icon == null)
                    continue;

                _lookup.Add(entry.tag, entry.icon);
                _orderedIcons.Add(entry.icon);

                entry.icon.SetActive(false);
            }
        }
        
        public void Add(GameplayTag tag)    // TODO: later this becomes SetStatusActive(effect.definition, true/false, effect.RemainingDuration, etc)
        {
            if (tag == null)
                return;

            if(!_lookup.TryGetValue(tag, out var icon))
                return;  // Ignore non-icon tags
            
            if(icon.activeSelf)
                return;  // Avoid unnecessary rebuilds, might need to change this if we handle stacks later.
            
            icon.SetActive(true);

            RefreshLayout();
        }

        public void Remove(GameplayTag tag)
        {
            if (tag == null)
                return;

            if(!_lookup.TryGetValue(tag, out var icon))
                return;
            
            if(!icon.activeSelf)
                return;  // Avoid unnecessary rebuilds, might need to change this if we handle stacks later.
            
            icon.SetActive(false);

            RefreshLayout();
        }

        private void RefreshLayout()
        {
            float x = 0f;

            foreach(var icon in _orderedIcons)
            {
                if (!icon.activeSelf)
                    continue;

                float width = GetIconWidth(icon);

                icon.transform.localPosition = new Vector3(
                    x,
                    0f,
                    0f);

                x += width + spacing;
                
                // 32x32 icons → 32 + padding
                // 64x64 icons → 64 + padding
            }
            /*int index = 0;

            foreach(var icon in _orderedIcons)  // orderedIcons guarantees the same presentation sequence if we refresh twice
            {
                if (!icon.activeSelf)
                    continue;

                icon.transform.localPosition =
                    new Vector3(
                        index * padding,
                        0f,
                        0f);

                index++;
            }*/
        }
        
        private float GetIconWidth(GameObject icon)
        {
            var renderer = icon.GetComponentInChildren<SpriteRenderer>();

            if (!renderer)
                return 0f;

            return renderer.sprite.bounds.size.x *  // convert to localspace to avoid breaking in case icon scale is different than 1
                   icon.transform.localScale.x;
            //if (icon.TryGetComponent<SpriteRenderer>(out var sprite))
            //    return sprite.bounds.size.x;

            //return 0.5f; // fallback
        }
    }
}
