using Unity.Collections;
using UnityEditor;
using UnityEngine;
using ReadOnlyAttribute = Core.Util.ReadOnlyAttribute;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Disables the GUI, making the field non-interactable
            GUI.enabled = false; 
        
            // Draws the property exactly as Unity normally would
            EditorGUI.PropertyField(position, property, label);
        
            // Re-enables the GUI so subsequent fields aren't locked
            GUI.enabled = true; 
        }
    }
}