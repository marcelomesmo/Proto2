// Assets/Game/Editor/SceneShortcutMenu.cs

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Game.Editor
{
    public static class SceneShortcutMenu
    {
        private const string MainMenuScenePath =
            "Assets/Scenes/MainMenu.unity";

        private const string LevelScenePath =
            "Assets/Scenes/Levels/Level_01.unity";

        [MenuItem("Game/Open Scene/Main Menu")]
        public static void OpenMainMenu()
        {
            OpenScene(MainMenuScenePath);
        }

        [MenuItem("Game/Open Scene/Level Scene")]
        public static void OpenLevelScene()
        {
            OpenScene(LevelScenePath);
        }

        private static void OpenScene(string scenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(scenePath);
        }
    }
}
#endif