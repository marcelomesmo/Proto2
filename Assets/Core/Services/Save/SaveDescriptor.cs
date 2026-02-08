using UnityEngine;

namespace Core.Services.Save
{
    [CreateAssetMenu(fileName = "SaveDescriptor", menuName = "Save/Descriptor")]
    public sealed class SaveDescriptor : ScriptableObject
    {
        [Header("Identity")]
        public string saveId = "main_profile";

        [Header("Local Storage")]
        public string localFileName = "game_save.json";
        public int platformSlot = 0;

        [Header("Versioning")]
        public int version = 1;
    }
}