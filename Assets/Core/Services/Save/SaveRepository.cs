using System.IO;
using UnityEngine;

namespace Core.Services.Save
{
    /*
     * SaveLocation
     * Responsible for Save/Load of persistent data.
     */
    public static class SaveRepository
    {
        public static string GetPath(string fileName)
        {
            return Path.Combine(
                Application.persistentDataPath,
                fileName
            );
        }

        public static void Save(string fileName, string json)
        {
            var path = GetPath(fileName);
            File.WriteAllText(path, json);
        }

        public static string Load(string fileName)
        {
            var path = GetPath(fileName);

            if (!File.Exists(path))
                return null;

            return File.ReadAllText(path);
        }

        public static void Delete(string fileName)
        {
            var path = GetPath(fileName);

            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
