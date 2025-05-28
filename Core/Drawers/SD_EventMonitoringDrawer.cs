#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SDSDInk.EventWeaver{
    [CustomPropertyDrawer(typeof(EventMonitoring))]
    public class SD_EventMonitoringDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Start drawing the property
            EditorGUI.BeginProperty(position, label, property);
            GUIStyle boldLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold
            };
            position.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(position, "Event Weaver Advanced Settings", boldLabelStyle);

            EditorGUI.indentLevel++;
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();

            SerializedProperty ewo = property.FindPropertyRelative("EventWeaverObjects");

            position.y += EditorGUIUtility.singleLineHeight;

            while (iterator.NextVisible(true) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                if (ewo.propertyPath != iterator.propertyPath)
                    continue;
                position.height = EditorGUI.GetPropertyHeight(iterator, true);
                EditorGUI.PropertyField(position, iterator, true);
                position.y += position.height + EditorGUIUtility.standardVerticalSpacing;
            }

            EditorGUI.indentLevel--;

            // End drawing the property
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Calculate total height of expanded property
            float height = EditorGUIUtility.singleLineHeight;
            SerializedProperty iterator = property.Copy();
            SerializedProperty endProperty = iterator.GetEndProperty();

            SerializedProperty ewo = property.FindPropertyRelative("EventWeaverObjects");

            while (iterator.NextVisible(true) && !SerializedProperty.EqualContents(iterator, endProperty))
            {
                if (ewo.propertyPath != iterator.propertyPath)
                    continue;
                height += EditorGUI.GetPropertyHeight(iterator, true) + EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }
    }
}
#endif