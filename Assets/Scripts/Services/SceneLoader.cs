using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Services
{
    public static class SceneLoader
    {
        private static MonoBehaviour _runner;
        
        private static string _pendingScene;

        // Must be called once (from Bootstrapper)
        public static void Initialize(MonoBehaviour coroutineRunner)
        {
            _runner = coroutineRunner;
        }

        // ---------- PUBLIC API ----------

        public static void LoadMenu(Action<float> onProgress = null)
        {
            LoadSceneAsync("MainMenu", onProgress);
        }

        public static void LoadLevel(string levelName, Action<float> onProgress = null)
        {
            LoadSceneAsync(levelName, onProgress);
        }

        /*
         * Used for:
            - Bootstrap → Menu
            - Editor utilities
            - Debug commands
            - Hard resets
         */
        public static void LoadSceneImmediate(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        
        // Called by Loading scene
        /*
         * Used for:
            - Player facing UX
         */
        public static void BeginLoadingFromLoadingScene(
            Action<float> onProgress)
        {
            if (string.IsNullOrEmpty(_pendingScene))
            {
                Debug.LogError("[SceneLoader] No pending scene set.");
                return;
            }

            if (_runner == null)
            {
                Debug.LogError("[SceneLoader] Not initialized.");
                SceneManager.LoadScene(_pendingScene);
                return;
            }

            _runner.StartCoroutine(
                LoadRoutine(_pendingScene, onProgress)
            );
        }

        // ---------- INTERNAL ----------
        
        /*
         * Used for:
            - Simple transitions
            - Internal scene swaps
            - Future additive loading
         */
        private static void LoadSceneAsync(string sceneName, Action<float> onProgress)
        {
            if (_runner == null)
            {
                Debug.LogError("[SceneLoader] Not initialized. Falling back to sync load.");
                SceneManager.LoadScene(sceneName);
                return;
            }

            _runner.StartCoroutine(LoadRoutine(sceneName, onProgress));
        }

        private static IEnumerator LoadRoutine(string sceneName, Action<float> onProgress)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                // This can be used to draw Player feedback
                onProgress?.Invoke(op.progress);
                yield return null;
            }

            // Finalize
            onProgress?.Invoke(1f);
            op.allowSceneActivation = true;
            
            _pendingScene = null;
        }
    }
}