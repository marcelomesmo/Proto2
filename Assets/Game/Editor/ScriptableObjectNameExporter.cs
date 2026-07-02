#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public static class ScriptableObjectNameExporter
    {
        [MenuItem("Game/Tools.Export/Copy Selected ScriptableObject Names")]
        private static void CopySelectedScriptableObjectNames()
        {
            var scriptableObjects = Selection.objects
                .OfType<ScriptableObject>()
                .OrderBy(x => x.name)
                .ToList();

            if (scriptableObjects.Count == 0)
            {
                Debug.LogWarning("No ScriptableObjects selected.");
                return;
            }

            var text = string.Join("\n", scriptableObjects.Select(x => x.name));
            EditorGUIUtility.systemCopyBuffer = text;

            Debug.Log($"Copied {scriptableObjects.Count} ScriptableObject names to clipboard.");
        }

        [MenuItem("Game/Tools.Export/Export ScriptableObject Names From Selection")]
        private static void ExportScriptableObjectNamesFromSelection()
        {
            var scriptableObjects = GetScriptableObjectsFromSelection();

            if (scriptableObjects.Count == 0)
            {
                Debug.LogWarning("No ScriptableObjects found in the current selection.");
                return;
            }

            var path = EditorUtility.SaveFilePanel(
                "Export ScriptableObject Names",
                Application.dataPath,
                "scriptable_object_names.csv",
                "csv");

            if (string.IsNullOrWhiteSpace(path))
                return;

            var csv = BuildCsv(scriptableObjects);
            File.WriteAllText(path, csv, Encoding.UTF8);

            Debug.Log($"Exported {scriptableObjects.Count} ScriptableObject names to: {path}");
            EditorUtility.RevealInFinder(path);
        }

        private static List<ScriptableObject> GetScriptableObjectsFromSelection()
        {
            var results = new List<ScriptableObject>();
            var addedPaths = new HashSet<string>();

            foreach (var guid in Selection.assetGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (string.IsNullOrWhiteSpace(assetPath))
                    continue;

                // If selection is a folder, find all ScriptableObjects inside it recursively.
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    var guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { assetPath });

                    foreach (var soGuid in guids)
                    {
                        var soPath = AssetDatabase.GUIDToAssetPath(soGuid);

                        if (!addedPaths.Add(soPath))
                            continue;

                        var so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(soPath);

                        if (so != null)
                            results.Add(so);
                    }

                    continue;
                }

                // If selection is directly a ScriptableObject asset.
                var selectedSo = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

                if (selectedSo != null && addedPaths.Add(assetPath))
                    results.Add(selectedSo);
            }

            return results
                .OrderBy(x => AssetDatabase.GetAssetPath(x))
                .ToList();
        }

        private static string BuildCsv(List<ScriptableObject> scriptableObjects)
        {
            var builder = new StringBuilder();

            builder.AppendLine("Name,AssetPath,Type");

            foreach (var so in scriptableObjects)
            {
                var assetPath = AssetDatabase.GetAssetPath(so);
                var typeName = so.GetType().Name;

                builder.AppendLine(
                    $"{EscapeCsv(so.name)},{EscapeCsv(assetPath)},{EscapeCsv(typeName)}");
            }

            return builder.ToString();
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            var mustQuote =
                value.Contains(",") ||
                value.Contains("\"") ||
                value.Contains("\n") ||
                value.Contains("\r");

            value = value.Replace("\"", "\"\"");

            return mustQuote ? $"\"{value}\"" : value;
        }
    }
}

#endif