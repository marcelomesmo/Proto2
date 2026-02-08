#if UNITY_EDITOR

using Core.Services.Save;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public static class SaveDebugMenu
    {
        [MenuItem("Game/Save/Delete Save File")]
        public static void DeleteSave()
        {
            /*if (!EditorUtility.DisplayDialog(
                    "Delete Save",
                    "Delete current save file?",
                    "Yes",
                    "No"))
                return;*/
            
            // 1. Locate descriptor
            var guids = AssetDatabase.FindAssets("t:SaveDescriptor");

            if (guids.Length == 0)
            {
                Debug.LogError(
                    "[SaveTools] No SaveDescriptor found.");
                return;
            }

            // Later: iterate over the list of descriptors if we want to clear everything.
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            
            // 2. Check if the descriptor is valid.
            var descriptor =
                AssetDatabase.LoadAssetAtPath<SaveDescriptor>(path);

            if (!descriptor)
            {
                Debug.LogError(
                    "[SaveTools] Failed to load descriptor.");
                return;
            }
            
            // 3. Delete the file.
            SaveRepository.Delete(descriptor.localFileName);

            Debug.Log($"[Save] Deleted: {descriptor.localFileName}");
        }
    }
}

#endif