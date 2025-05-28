using UnityEngine;
using UnityEditor;

namespace SDSDInk.EventWeaver
{
    [CustomEditor(typeof(SD_EventManager))]
    public class SD_EventManagerEditor : Editor
    {
        Texture2D myTexture;

        public override void OnInspectorGUI()
        {
            if (myTexture == null)
                myTexture = FindTextureByName("EWLBanner");
            // Draw the image at the top
            if (myTexture != null)
            {
                float aspectRatio = (float)myTexture.height / myTexture.width;
                float width = EditorGUIUtility.currentViewWidth - 20; // Adjust width to fit the Inspector
                float height = width * aspectRatio;

                Rect rect = GUILayoutUtility.GetRect(width, height);
                EditorGUI.DrawPreviewTexture(rect, myTexture);
            }

            EditorGUILayout.Space();

            // Begin checking properties
            SerializedProperty property = serializedObject.GetIterator();
            property.NextVisible(true); // Skip the "m_Script" field

            // Draw the remaining properties
            while (property.NextVisible(false))
            {
                EditorGUILayout.PropertyField(property, true);
            }

            // Apply changes to serialized properties
            serializedObject.ApplyModifiedProperties();
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