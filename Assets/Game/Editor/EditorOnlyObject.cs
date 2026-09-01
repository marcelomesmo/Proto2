using UnityEngine;

namespace Game.Editor
{
    public class EditorOnlyObject : MonoBehaviour
    {
#if !UNITY_EDITOR
        private void Awake()
        {
            gameObject.SetActive(false);
        }
#endif
    }
}