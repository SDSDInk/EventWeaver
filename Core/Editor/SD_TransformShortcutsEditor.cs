using UnityEngine;
using UnityEditor;

namespace SDSDInk.EventWeaver
{
    [CustomEditor(typeof(SD_TransformShortcuts))]
    public class SD_TransformShortcutsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Begin checking properties
            SerializedProperty property = serializedObject.GetIterator();

            property.NextVisible(true); // Skip the "m_Script" field

            // Draw the remaining properties

            while (property.NextVisible(false))
            {
                // Draw other properties normally
                EditorGUILayout.PropertyField(property, true);
            }

            // Apply changes to serialized properties
            serializedObject.ApplyModifiedProperties();
        }
    }
}