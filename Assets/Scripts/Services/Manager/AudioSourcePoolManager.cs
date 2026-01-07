using System.Collections.Generic;
using Interfaces;
using UnityEngine;

namespace Services.Manager
{
    public class AudioSourcePoolManager : MonoBehaviour, IPool
    {
        private readonly Queue<AudioSource> _pool = new();
        private readonly Transform _parent;
        private readonly HashSet<AudioSource> _activeSources = new();

        public AudioSourcePoolManager(int size, Transform parent)
        {
            _parent = parent;

            for (int i = 0; i < size; i++)
                _pool.Enqueue(CreateSource());
        }

        private AudioSource CreateSource()
        {
            GameObject go = new GameObject("PooledAudioSource");
            go.transform.parent = _parent;

            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1f;

            go.SetActive(false);
            return source;
        }

        public AudioSource Get()
        {
            AudioSource src = _pool.Count > 0
                ? _pool.Dequeue()
                : CreateSource();

            src.gameObject.SetActive(true);
            _activeSources.Add(src);
            return src;
        }

        public void Release(AudioSource source)
        {
            if (source == null)
                return;

            if (_activeSources.Remove(source))
            {
                source.Stop();
                source.clip = null;
                source.outputAudioMixerGroup = null;
                source.gameObject.SetActive(false);
                _pool.Enqueue(source);
            }
        }

        public void ReleaseAll()
        {
            foreach (var source in _activeSources)
            {
                if (!source)
                    continue;

                source.Stop();
                source.clip = null;
                source.outputAudioMixerGroup = null;
                source.gameObject.SetActive(false);
                _pool.Enqueue(source);
            }

            _activeSources.Clear();
        }
    }
}
