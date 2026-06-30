#if UNITY_EDITOR

using System.Collections.Generic;
using Core.Services.Save;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public static class SaveDebugMenu
    {
        [MenuItem("Game/Save/Delete Save File")]
        public static void DeleteAllSaves()
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

            var deletedFiles = new HashSet<string>();
            int deletedCount = 0;
            int skippedCount = 0;

            // 2. Iterate all descriptors
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                
                // 2.1. Check if the descriptor is valid.
                var descriptor =
                    AssetDatabase.LoadAssetAtPath<SaveDescriptor>(path);

                if (!descriptor)
                {
                    Debug.LogWarning($"[SaveTools] Failed to load SaveDescriptor at path: {path}");
                    skippedCount++;
                    continue;
                }
                
                if (string.IsNullOrWhiteSpace(descriptor.localFileName))
                {
                    Debug.LogWarning($"[SaveTools] SaveDescriptor has no local file name: {path}");
                    skippedCount++;
                    continue;
                }

                // Avoid deleting/logging the same save file twice if two descriptors point to it.
                if (!deletedFiles.Add(descriptor.localFileName))
                {
                    Debug.LogWarning($"[SaveTools] Duplicate save file skipped: {descriptor.localFileName}");
                    skippedCount++;
                    continue;
                }
                
                // 2.2. Delete the file.
                SaveRepository.Delete(descriptor.localFileName);
                deletedCount++;

                Debug.Log($"[SaveTools] Deleted save file: {descriptor.localFileName}");
            }
           
            Debug.Log($"[SaveTools] Delete complete. Deleted: {deletedCount}. Skipped: {skippedCount}.");
        }
    }
}

#endif