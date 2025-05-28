using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

namespace SDSDInk.EventWeaver
{
    public class ComponentReplacerWizard : EditorWindow
    {
        private Component oldComponent; // The old component to be replaced
        private MonoScript newComponentScript; // The script for the new component

        [MenuItem("Window/Event Weaver/Component Replacer Wizard")]
        public static void ShowWindow()
        {
            GetWindow<ComponentReplacerWizard>("Component Replacer Wizard");
        }

        private void OnGUI()
        {
            GUILayout.Label("Component Replacer Wizard", EditorStyles.boldLabel);

            // Display the selected GameObject
            if (Selection.activeGameObject != null)
            {
                GUILayout.Label($"Selected GameObject: {Selection.activeGameObject.name}");

                // Input fields
                oldComponent = EditorGUILayout.ObjectField("Old Component", oldComponent, typeof(Component), true) as Component;
            }
            else
            {
                GUILayout.Label("No GameObject selected.");
            }
            newComponentScript = EditorGUILayout.ObjectField("New Component Script", newComponentScript, typeof(MonoScript), false) as MonoScript;

            if (GUILayout.Button("Replace Component"))
            {
                if (ValidateInputs())
                {
                    ReplaceComponentWithSerializedData();
                }
            }
        }

        private bool ValidateInputs()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogError("No GameObject selected.");
                return false;
            }

            if (oldComponent == null)
            {
                Debug.LogError("No old component selected.");
                return false;
            }

            if (newComponentScript == null)
            {
                Debug.LogError("No new component script selected.");
                return false;
            }

            // Ensure the new script corresponds to a valid component type
            Type newComponentType = newComponentScript.GetClass();
            if (newComponentType == null || !newComponentType.IsSubclassOf(typeof(Component)))
            {
                Debug.LogError("Selected script is not a valid Component.");
                return false;
            }

            return true;
        }

        private void ReplaceComponentWithSerializedData()
        {
            GameObject targetGameObject = oldComponent.gameObject;

            // Get the type of the new component from the MonoScript
            Type newComponentType = newComponentScript.GetClass();
            if (newComponentType == null || !newComponentType.IsSubclassOf(typeof(Component)))
            {
                Debug.LogError("Selected script is not a valid Component.");
                return;
            }

            // Add the new component
            Component newComponent = targetGameObject.AddComponent(newComponentType);

            // Transfer serialized data
            SerializedObject oldSerializedObject = new SerializedObject(oldComponent);
            SerializedObject newSerializedObject = new SerializedObject(newComponent);

            SerializedProperty oldProperty = oldSerializedObject.GetIterator();
            while (oldProperty.NextVisible(true))
            {
                if (oldProperty.propertyPath == "m_Script") // Skip the script reference
                    continue;

                SerializedProperty newProperty = newSerializedObject.FindProperty(oldProperty.propertyPath);
                if (newProperty != null && newProperty.propertyType == oldProperty.propertyType)
                {
                    newProperty.serializedObject.CopyFromSerializedProperty(oldProperty);
                }
            }

            newSerializedObject.ApplyModifiedProperties();

            // Remove the old component
            DestroyImmediate(oldComponent);

            Debug.Log($"Replaced {oldComponent.GetType().Name} with {newComponentType.Name} on {targetGameObject.name}.");
        }
    }
}