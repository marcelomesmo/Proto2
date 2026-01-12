#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Core.Editor
{
    public static class DevSceneLauncher
    {
        [MenuItem("Dev/Play From Bootstrap")]
        public static void PlayFromBootstrap()
        {
            EditorApplication.isPlaying = false;
            EditorSceneManager.OpenScene("Assets/Scenes/Bootstrap.unity");
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Dev/Play From Menu")]
        public static void PlayFromMenu()
        {
            EditorApplication.isPlaying = false;
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Dev/Play From Level")]
        public static void PlayFromLevel()
        {
            EditorApplication.isPlaying = false;
            EditorSceneManager.OpenScene("Assets/Scenes/Levels/Level_01.unity");
            EditorApplication.isPlaying = true;
        }
    }
}
#endif