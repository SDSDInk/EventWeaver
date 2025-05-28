using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Linq;
using static SDSDInk.EventWeaver.SD_AStarAI;

namespace SDSDInk.EventWeaver
{
    [CustomPropertyDrawer(typeof(AIStateEvents))]
    public class AIStateEventsDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, label, true);

            if (!property.isExpanded)
                return;

            EditorGUI.indentLevel++;

            string[][] eventGroups = new string[][]
            {
                new[] { "onThinking", "OnThinkingBegan", "WhileThinking", "OnThinkingComplete" },
                new[] { "onLookingForWanderPoint", "OnLookingForWanderPointBegan", "WhileLookingForWanderPoint", "OnLookingForWanderPointComplete" },
                new[] { "onFollowingPathToGoal", "OnFollowingPathToGoalBegan", "WhileFollowingPathToGoal", "OnFollowingPathToGoalComplete" },
                new[] { "onFoundTarget", "OnFoundTargetBegan", "WhileFoundTarget", "OnFoundTargetComplete" },
                new[] { "onChasingTarget", "OnChasingTargetBegan", "WhileChasingTarget", "OnChasingTargetComplete" },
                new[] { "onWithinStoppingRange", "OnWithinStoppingRangeBegan", "WhileWithinStoppingRange", "OnWithinStoppingRangeComplete" },
                new[] { "onAttackingTarget", "OnAttackingTargetBegan", "WhileAttackingTarget", "OnAttackingTargetComplete" },
                new[] { "onTargetEliminated", "OnTargetEliminatedBegan", "WhileTargetEliminated", "OnTargetEliminatedComplete" },
                new[] { "onGoingToLastKnownPosition", "OnGoingToLastKnownPositionBegan", "WhileGoingToLastKnownPosition", "OnGoingToLastKnownPositionComplete" },
                new[] { "onTargetLost", "OnTargetLostBegan", "WhileTargetLost", "OnTargetLostComplete" },
                new[] { "onArrivedAtFinalLocation", "OnArrivedAtFinalLocationBegan", "WhileArrivedAtFinalLocation", "OnArrivedAtFinalLocationComplete" }
            };

            float yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            Rect fieldRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);

            foreach (var prop in eventGroups)
            {
                SerializedProperty toggle = property.FindPropertyRelative(prop[0]);

                toggle.boolValue = EditorGUI.Foldout(fieldRect, toggle.boolValue, toggle.displayName);
                fieldRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                if (toggle.boolValue)
                {
                    SerializedProperty onThinkingBegan = property.FindPropertyRelative(prop[1]);
                    SerializedProperty whileThinking = property.FindPropertyRelative(prop[2]);
                    SerializedProperty onThinkingComplete = property.FindPropertyRelative(prop[3]);

                    EditorGUI.indentLevel++;
                    EditorGUI.PropertyField(fieldRect, onThinkingBegan);
                    fieldRect.y += EditorGUI.GetPropertyHeight(onThinkingBegan, true);
                    EditorGUI.PropertyField(fieldRect, whileThinking);
                    fieldRect.y += EditorGUI.GetPropertyHeight(whileThinking, true);
                    EditorGUI.PropertyField(fieldRect, onThinkingComplete);
                    fieldRect.y += EditorGUI.GetPropertyHeight(onThinkingComplete, true);
                    EditorGUI.indentLevel--;
                }           
            }

            EditorGUI.indentLevel--;
            fieldRect.y += 100;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Start with the height of the label
            float totalHeight = EditorGUIUtility.singleLineHeight;

            // If expanded, calculate height for the fields
            if (property.isExpanded)
            {
                string[][] eventGroups = new string[][]
                {
                    new[] { "onThinking", "OnThinkingBegan", "WhileThinking", "OnThinkingComplete" },
                    new[] { "onLookingForWanderPoint", "OnLookingForWanderPointBegan", "WhileLookingForWanderPoint", "OnLookingForWanderPointComplete" },
                    new[] { "onFollowingPathToGoal", "OnFollowingPathToGoalBegan", "WhileFollowingPathToGoal", "OnFollowingPathToGoalComplete" },
                    new[] { "onFoundTarget", "OnFoundTargetBegan", "WhileFoundTarget", "OnFoundTargetComplete" },
                    new[] { "onChasingTarget", "OnChasingTargetBegan", "WhileChasingTarget", "OnChasingTargetComplete" },
                    new[] { "onWithinStoppingRange", "OnWithinStoppingRangeBegan", "WhileWithinStoppingRange", "OnWithinStoppingRangeComplete" },
                    new[] { "onAttackingTarget", "OnAttackingTargetBegan", "WhileAttackingTarget", "OnAttackingTargetComplete" },
                    new[] { "onTargetEliminated", "OnTargetEliminatedBegan", "WhileTargetEliminated", "OnTargetEliminatedComplete" },
                    new[] { "onGoingToLastKnownPosition", "OnGoingToLastKnownPositionBegan", "WhileGoingToLastKnownPosition", "OnGoingToLastKnownPositionComplete" },
                    new[] { "onTargetLost", "OnTargetLostBegan", "WhileTargetLost", "OnTargetLostComplete" },
                    new[] { "onArrivedAtFinalLocation", "OnArrivedAtFinalLocationBegan", "WhileArrivedAtFinalLocation", "OnArrivedAtFinalLocationComplete" }
                };

                foreach (var prop in eventGroups)
                {
                    SerializedProperty toggle = property.FindPropertyRelative(prop[0]);

                    totalHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    if (toggle.boolValue) // Add height for expanded properties
                    {
                        // Add 3 properties for each expanded group
                        for (int i = 1; i < 4; i++)
                        {
                            totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative(prop[i]), true);
                        }
                    }
                }
            }

            return totalHeight;
        }
    }

}