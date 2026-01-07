using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Gameplay.Entity.Tags
{
    public class TagSystem
    {
        private readonly HashSet<GameplayTag> _activeTags = new();

        public event Action<GameplayTag> OnTagAdded;
        public event Action<GameplayTag> OnTagRemoved;

        public bool HasTag(GameplayTag tag) => _activeTags.Contains(tag);

        public void AddTag(GameplayTag tag)
        {
            if (_activeTags.Add(tag))
                OnTagAdded?.Invoke(tag);
        }

        public void RemoveTag(GameplayTag tag)
        {
            if (_activeTags.Remove(tag))
                OnTagRemoved?.Invoke(tag);
        }

        public void ClearTags()
        {
            _activeTags.Clear();
        }
        
        public void ClearTemporaryTags()
        {
            // Snapshot because RemoveTag mutates the collection
            var snapshot = _activeTags.ToArray();

            foreach (var tag in snapshot)
            { 
                // Safe-guard for null tags.
                if (!tag)
                {
                    Debug.LogWarning("[TagSystem] Null tag encountered during ClearTemporaryTags.");
                    continue;
                }

                if (tag.lifetime == TagLifetime.Temporary)
                    RemoveTag(tag);
            }
        }
    }
}