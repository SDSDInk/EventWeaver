using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;

namespace SDSDInk.EventWeaver
{
    [CustomEditor(typeof(SD_Event))]
    public class SD_EventEditor : Editor
    {
        Texture2D myTexture;
        Vector2 scrollPos;
        int columns = 4;

        public override void OnInspectorGUI()
        {
            if (myTexture == null)
                myTexture = FindTextureByName("EWLEvent");

            float inspectorWidth = EditorGUIUtility.currentViewWidth;

            // Draw the image at the top
            if (myTexture != null)
            {
                // Get the texture's original size
                float originalWidth = myTexture.width;
                float originalHeight = myTexture.height;

                // Clamp the width to the smaller of the inspector width minus padding or the texture's original width
                float maxWidth = Mathf.Min(originalWidth, inspectorWidth - 20);
                float aspectRatio = originalHeight / originalWidth;

                // Calculate the height while maintaining the aspect ratio
                float height = maxWidth * aspectRatio;

                // Calculate the horizontal offset to center the texture
                float offsetX = (inspectorWidth - maxWidth) / 2;

                // Add a vertical space and create a centered Rect
                Rect rect = new Rect(offsetX, GUILayoutUtility.GetRect(maxWidth, height).y, maxWidth, height);

                EditorGUI.DrawPreviewTexture(rect, myTexture);
            }

            // a dropdown that adds a component (if the component isalready on this object)
            MonoBehaviour targetObject = (MonoBehaviour)target;

            if (targetObject == null) return;

            EditorGUILayout.Space();

            // Begin checking properties
            SerializedProperty property = serializedObject.GetIterator();
            SerializedProperty onTriggered = serializedObject.FindProperty("onTriggered");
            SerializedProperty onActivePageNull = serializedObject.FindProperty("onActivePageNull");
            SerializedProperty onEmptied = serializedObject.FindProperty("onEmptied");
            SerializedProperty events = serializedObject.FindProperty("events");
            SerializedProperty debugMode = serializedObject.FindProperty("debugMode");
            SerializedProperty pagesToShow = serializedObject.FindProperty("pagesToShow");

            property.NextVisible(true); // Skip the "m_Script" field

            // Draw the remaining properties

            while (property.NextVisible(false))
            {
                bool showProperty = true;

                bool isGlobalTrigger = 
                    property.propertyPath == onTriggered.propertyPath ||
                    property.propertyPath == onActivePageNull.propertyPath ||
                    property.propertyPath == onEmptied.propertyPath;

                if (property.propertyPath == onTriggered.propertyPath)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(SD_EWStaticEditor.showTriggerEvents ? "Hide Trigger Events" : "Show Trigger Events", GUILayout.Width(100)); // Adjust the width as needed
                    SD_EWStaticEditor.showTriggerEvents = EditorGUILayout.Toggle(SD_EWStaticEditor.showTriggerEvents);
                    GUILayout.EndHorizontal();
                }

                if (!SD_EWStaticEditor.showTriggerEvents)                
                    showProperty = !isGlobalTrigger;

                if (showProperty)
                {
                    if (!debugMode.boolValue && property.propertyPath == events.propertyPath)
                    {
                        // Get the available inspector width
                        float buttonWidth = (inspectorWidth - 75) / columns;

                        // Initialize pagesToShow only once
                        if (pagesToShow == null || pagesToShow.arraySize != events.arraySize)
                        {
                            // Clear the current array
                            pagesToShow.ClearArray();

                            // Resize the array to match the size of 'events'
                            pagesToShow.arraySize = events.arraySize;

                            // to retain the values from the previous array
                            for (int i = 0; i < Mathf.Min(pagesToShow.arraySize, events.arraySize); i++)
                            {
                                // Copy the old values (if present)
                                if (i < pagesToShow.arraySize)
                                {
                                    pagesToShow.GetArrayElementAtIndex(i).boolValue = pagesToShow.GetArrayElementAtIndex(i).boolValue;
                                }
                            }

                            // Fill the remaining elements with default values (false in this case)
                            for (int i = Mathf.Min(pagesToShow.arraySize, events.arraySize); i < events.arraySize; i++)
                            {
                                pagesToShow.GetArrayElementAtIndex(i).boolValue = false;
                            }
                        }
                        int rows = Mathf.CeilToInt((float)events.arraySize / columns); // Calculate the number of rows
                        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
                        {
                            alignment = TextAnchor.MiddleLeft  // Align the text inside the button to the left
                        };

                        EditorGUILayout.BeginHorizontal(GUI.skin.box);
                        if (GUILayout.Button(new GUIContent("New Page", "Create new event page"), buttonStyle, GUILayout.Width(100)))
                        {
                            // Increment the size of the array
                            events.arraySize++;

                            // Access the newly added element
                            var newElement = events.GetArrayElementAtIndex(events.arraySize - 1);

                            // Apply the changes to the serialized object
                            events.serializedObject.ApplyModifiedProperties();
                            return;
                        }

                        EditorGUILayout.Space();
                        GUILayout.Label(new GUIContent("Buttons/Row"), GUILayout.MaxWidth(85));
                        string columnsString = columns.ToString();
                        // Create a horizontal layout for each row
                        columnsString = GUILayout.TextField(columnsString, GUILayout.Width(200));

                        // Validate the input to ensure it's a valid number and greater than 0
                        if (int.TryParse(columnsString, out int parsedValue))
                        {
                            // Ensure the number is greater than 0
                            if (parsedValue > 1)
                            {
                                columns = parsedValue; // Update the `columns` variable
                            }
                            else
                            {
                                columns = 2; // Enforce a minimum value of 1
                            }
                        }
                        else
                        {
                            columns = 2; // Reset to a valid value if the input is invalid
                        }
                        GUILayout.EndHorizontal(); // End the row

                        string alert = "No Active Page: In Editor Mode ";
                        for (int i = 0; i < events.arraySize; i++)
                        {
                            var e = events.GetArrayElementAtIndex(i);
                            if (e.FindPropertyRelative("isActivePage").boolValue)
                            {
                                var a = e.FindPropertyRelative("page").stringValue;
                                alert = "Active Page is: " + a;
                                break;
                            }
                        }
                        GUILayout.Label(new GUIContent(alert));

                        // Begin the scroll view (if needed)
                        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(rows * (EditorGUIUtility.singleLineHeight * 2f)));

                        for (int row = 0; row < rows; row++)
                        {
                            EditorGUILayout.BeginHorizontal();
                            // Loop through columns in each row
                            for (int col = 0; col < columns; col++)
                            {
                                int index = row * columns + col;
                                if (index >= events.arraySize) break; // Break if we exceed the number of events

                                // Get the element at index
                                SerializedProperty element = events.GetArrayElementAtIndex(index);

                                EditorGUILayout.BeginHorizontal(GUI.skin.box);
                                // Button to toggle display of the event
                                if (GUILayout.Button(
                                    new GUIContent(
                                        $"{element.FindPropertyRelative("page").stringValue}", 
                                        $" {element.FindPropertyRelative("page").stringValue} "
                                    ), 
                                    buttonStyle, GUILayout.Width(buttonWidth - 26)))
                                {
                                    // Toggle the visibility of the corresponding page
                                    for (int i = 0; i < pagesToShow.arraySize; i++)
                                        pagesToShow.GetArrayElementAtIndex(i).boolValue = false;
                                    pagesToShow.GetArrayElementAtIndex(index).boolValue = true;
                                    element.isExpanded = true;
                                }
                                GUI.backgroundColor = Color.red;
                                if (GUILayout.Button(new GUIContent("X", $" Delete: {element.FindPropertyRelative("page").stringValue} "), GUILayout.Width(20)))
                                {
                                    // Remove the last element from the array
                                    events.DeleteArrayElementAtIndex(index);

                                    // Apply the changes to the serialized object
                                    events.serializedObject.ApplyModifiedProperties();
                                }
                                GUI.backgroundColor = Color.white;
                                GUILayout.EndHorizontal(); // End the row
                            }
                            GUILayout.EndHorizontal(); // End the row
                        }

                        // End the scroll view (if used)
                        EditorGUILayout.EndScrollView();

                        for (int i = 0; i < events.arraySize; i++)
                        {
                            // Get the element at index i
                            SerializedProperty element = events.GetArrayElementAtIndex(i);

                            if (pagesToShow.GetArrayElementAtIndex(i).boolValue == true)
                            {
                                EditorGUILayout.BeginVertical(GUI.skin.box);
                                // Display the element in the Inspector
                                EditorGUILayout.PropertyField(element, new GUIContent("Editing " + element.FindPropertyRelative("page").stringValue), true);

                                EditorGUILayout.EndVertical();
                            }
                        }
                    }
                    else
                        EditorGUILayout.PropertyField(property, true);
                }
            }

            EditorGUILayout.Space();

            if (EditorGUILayout.DropdownButton(new GUIContent("Add Component"), FocusType.Keyboard))
            {
                GenericMenu menu = new GenericMenu();

                // Add components to the dropdown menu
                menu.AddSeparator("GameObject Management");
                AddComponentOption<SD_InstantiateGameObject>(menu, targetObject, true);
                AddComponentOption<SD_TransformShortcuts>(menu, targetObject, true);

                menu.AddSeparator("Conditional Logic");
                AddComponentOption<SD_IfStatement>(menu, targetObject, false);
                AddComponentOption<SD_WhileLoop>(menu, targetObject, false);
                AddComponentOption<SD_WaitForSeconds>(menu, targetObject, false);

                menu.AddSeparator("Switches and Variables");
                AddComponentOption<SD_ChangeStateSwitch>(menu, targetObject, false);
                AddComponentOption<SD_ChangeVariable>(menu, targetObject, false);

                menu.AddSeparator("Dialogue");
                AddComponentOption<SD_DialogueHandler>(menu, targetObject, false);

                menu.AddSeparator("Other");
                AddComponentOption<SD_SceneManagerRemote>(menu, targetObject, true);
                AddComponentOption<SD_DebugLog>(menu, targetObject, true);

                if (menu.GetItemCount() > 0)
                {
                    menu.ShowAsContext();
                }
                else
                {
                    Debug.Log("All components are already added to the GameObject.");
                }
            }

            // Apply changes to serialized properties
            serializedObject.ApplyModifiedProperties();
        }

        private void AddComponentOption<T>(GenericMenu menu, MonoBehaviour targetObject, bool justOne) where T : Component
        {
            // Check if the component is already on the object
            if (justOne && targetObject.GetComponent<T>() != null) return;

            string componentName = typeof(T).Name;

            // Add the menu option for the component
            menu.AddItem(new GUIContent(componentName), false, () =>
            {
                // Use Undo to add the component, allowing for undo functionality in the editor
                Undo.AddComponent<T>(targetObject.gameObject);
            });
        }

        private Texture2D FindTextureByName(string textureName)
        {
            // Find all assets with the specified name
            string[] guids = AssetDatabase.FindAssets(textureName + " t:Texture2D");

            // Ensure a texture was found
            if (guids.Length > 0)
            {
                // Load the first match found
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            else
            {
                Debug.LogWarning($"Texture named '{textureName}' not found.");
                return null;
            }
        }
    }
}