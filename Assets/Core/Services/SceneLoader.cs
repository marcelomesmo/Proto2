using System;
using System.Collections;
using Core.Enum;
using Core.Level;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Services
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

        public static void LoadSplashScreen(Action<float> onProgress = null)
        {
            // Fix: was LoadSceneAsync, should be immediate — no visuals to transition from
            //LoadSceneAsync("SplashScreen", onProgress);
            LoadSceneImmediate("SplashScreen");
        }
        
        public static void LoadMenu(Action<float> onProgress = null)
        {
            if(onProgress != null)
                LoadSceneAsync("MainMenu", onProgress);
            else
                LoadLevel("MainMenu");
        }

        // LoadLevel — unchanged signature, internally now reads LevelConfig
        public static void LoadLevel(string sceneName)
        {
            if (_runner == null)
            {
                Debug.LogError("[SceneLoader] Not initialized. Falling back to sync load.");
                SceneManager.LoadScene(sceneName);
                return;
            }
            
            _runner.StartCoroutine(LoadLevelWithTransition(sceneName));
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
        
        // New internal coroutine — replaces direct LoadRoutine call for levels
        private static IEnumerator LoadLevelWithTransition(string sceneName)
        {
            ServiceLocator.TryGet<SceneTransitionService>(out var transition);
            ServiceLocator.TryGet<LevelRegistry>(out var registry);

            string currentSceneName = SceneManager.GetActiveScene().name;
            LevelConfig outgoingConfig = null;

            if (registry == null)
                Debug.LogWarning($"[SceneLoader] LevelRegistry not available. Loading '{sceneName}' without config.");
            else if (!registry.TryGet(currentSceneName, out outgoingConfig))
                    Debug.LogWarning($"[SceneLoader] No LevelConfig for outgoing scene '{currentSceneName}'.");
            
            TransitionStyle style = outgoingConfig?.ExitTransitionStyle ?? TransitionStyle.None;
            float duration = outgoingConfig?.TransitionDuration ?? 0.4f;
            
            IEnumerator loadRoutine = AsyncLoadRoutine(sceneName);
            
            if (transition != null)
                yield return _runner.StartCoroutine(
                    transition.PlayFullTransition(style, duration, loadRoutine));
            else
                yield return _runner.StartCoroutine(loadRoutine);
        }
        
        private static IEnumerator AsyncLoadRoutine(string sceneName)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
                yield return null;

            op.allowSceneActivation = true;
            yield return null; // one frame for scene to settle
            
            // Always reset timescale before any scene transition.
            // Handles: paused exit, speed multiplier carry-over, any other timeScale state.
            // Last thing we do before the next scene is loaded.
            GameTimeService.ForceResetSpeed();
        }
    }
}