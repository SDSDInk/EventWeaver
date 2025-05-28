using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SDSDInk.EventWeaver{
    public static class SD_EventWeaverCommons
    {
        // used to clone a custom class list with a clone method
        public static List<T> CloneList<T>(List<T> originalList, Func<T, T> cloneFunc)
        {
            List<T> clonedList = new();
            foreach (T item in originalList)
            {
                var cf = cloneFunc(item);// runs the clone method and clones the variable into the list
                clonedList.Add(cf); 
            }

            return clonedList;
        }

        // generates an md5 hash out of the input string
        public static string GenerateMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to a hexadecimal string
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // gets the parameter types of a Unity Event
        public static Type[] GetParameterTypes(UnityEventBase unityEventBase, int listenerIndex)
        {
            // Retrieve the parameter types from the UnityEventBase
            var field = typeof(UnityEventBase).GetField("m_PersistentCalls", BindingFlags.NonPublic | BindingFlags.Instance);
            var persistentCalls = field?.GetValue(unityEventBase);

            if (persistentCalls != null)
            {
                var callsField = persistentCalls.GetType().GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
                var calls = callsField?.GetValue(persistentCalls) as System.Collections.IList;

                if (calls != null && listenerIndex < calls.Count)
                {
                    var call = calls[listenerIndex];
                    var methodField = call.GetType().GetField("m_MethodName", BindingFlags.NonPublic | BindingFlags.Instance);
                    var targetField = call.GetType().GetField("m_Target", BindingFlags.NonPublic | BindingFlags.Instance);

                    if (methodField != null && targetField != null)
                    {
                        var target = targetField.GetValue(call) as UnityEngine.Object;
                        var methodName = methodField.GetValue(call) as string;

                        // Safeguard: if target or methodName are invalid, treat it as null listener
                        if (target == null || string.IsNullOrEmpty(methodName))
                        {
                           //Debug.Log("target is null or : " + methodName + " is empty");
                            return Array.Empty<Type>();
                        }

                        try
                        {
                            // Get all methods with the given name
                            MethodInfo[] methods = target.GetType()
                                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                .Where(m => m.Name == methodName)
                                .ToArray();

                            // If no methods were found, return empty
                            if (methods.Length == 0)
                            {
                                return Array.Empty<Type>();
                            }


                            // Filter methods by parameter count first
                            MethodInfo matchedMethod = null;
                            foreach (var method in methods)
                            {
                                var parameters = method.GetParameters();
                                bool parametersMatch = parameters.Length > 0;

                                // Only check types if parameter count matches
                                if (parameters.Length > 0)
                                {
                                    //Debug.Log("Yes");
                                    parametersMatch = true;
                                    var eventParameters = parameters; // Parameters from the method
                                    for (int i = 0; i < eventParameters.Length; i++)
                                    {
                                        if (i >= eventParameters.Length || eventParameters[i].ParameterType != parameters[i].ParameterType)
                                        {
                                            parametersMatch = false;
                                            break;
                                        }
                                    }
                                }

                                // Select method if parameter count and types match
                                if (parametersMatch)
                                {
                                    matchedMethod = method;
                                    break; // Once we find the first match, exit the loop
                                }
                            }

                            // If matched method is found, return the parameter types
                            if (matchedMethod != null)
                            {
                                var parameters = matchedMethod.GetParameters();
                                return parameters.Select(p => p.ParameterType).ToArray();
                            }
                        }
                        catch (AmbiguousMatchException)
                        {
                            // Handle ambiguity gracefully
                            return Array.Empty<Type>();
                        }
                    }
                }
            }

            return Array.Empty<Type>();
        }

        // gets the parameter values from a unity Event
        public static object[] GetParameterValues(UnityEventBase unityEventBase, int listenerIndex, MethodInfo methodInfo)
        {
            // Use reflection to retrieve argument values
            var field = typeof(UnityEventBase).GetField("m_PersistentCalls", BindingFlags.NonPublic | BindingFlags.Instance);
            var persistentCalls = field?.GetValue(unityEventBase);

            if (persistentCalls != null)
            {
                var callsField = persistentCalls.GetType().GetField("m_Calls", BindingFlags.NonPublic | BindingFlags.Instance);
                var calls = callsField?.GetValue(persistentCalls) as System.Collections.IList;

                if (calls != null && listenerIndex < calls.Count)
                {
                    var call = calls[listenerIndex];
                    var argumentsField = call.GetType().GetField("m_Arguments", BindingFlags.NonPublic | BindingFlags.Instance);

                    //foreach (var parameterType in calls)
                    if (argumentsField != null)
                    {
                        var arguments = argumentsField.GetValue(call);

                        // Extract argument values based on parameter types
                        if (arguments != null)
                        {
                            var argsType = arguments.GetType();
                            var parameters = methodInfo.GetParameters();

                            object[] parameterValues = new object[parameters.Length];
                            for (int i = 0; i < parameters.Length; i++)
                            {
                                Type parameterType = parameters[i].ParameterType;

                                // Match the parameter type to the corresponding field
                                if (parameterType == typeof(int))
                                {
                                    var intField = argsType.GetField("m_IntArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (intField != null)
                                        parameterValues[i] = intField.GetValue(arguments);
                                }
                                else if (parameterType == typeof(float))
                                {
                                    var floatField = argsType.GetField("m_FloatArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (floatField != null)
                                        parameterValues[i] = floatField.GetValue(arguments);
                                }
                                else if (parameterType == typeof(string))
                                {
                                    var stringField = argsType.GetField("m_StringArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (stringField != null)
                                        parameterValues[i] = stringField.GetValue(arguments);
                                }
                                else if (parameterType == typeof(bool))
                                {
                                    var boolField = argsType.GetField("m_BoolArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    if (boolField != null)
                                        parameterValues[i] = boolField.GetValue(arguments);
                                }
                                else if (parameterType.IsSubclassOf(typeof(UnityEngine.Object)) || parameterType == typeof(UnityEngine.Object))
                                {
                                    // Handle UnityEngine.Object arguments
                                    var unityObjectField = argsType.GetField("m_ObjectArgument", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                                    var assemblyTypeNameField = argsType.GetField("m_ObjectArgumentAssemblyTypeName", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                                    UnityEngine.Object unityObject = null;
                                    string assemblyTypeName = null;

                                    if (unityObjectField != null)
                                        unityObject = unityObjectField.GetValue(arguments) as UnityEngine.Object;

                                    if (assemblyTypeNameField != null)
                                        assemblyTypeName = assemblyTypeNameField.GetValue(arguments) as string;

                                    if (unityObject != null)
                                    {
                                        if (!string.IsNullOrEmpty(assemblyTypeName))
                                        {
                                            Type expectedType = Type.GetType(assemblyTypeName);
                                            if (expectedType != null && parameterType.IsAssignableFrom(expectedType))
                                            {
                                                parameterValues[i] = unityObject;
                                            }
                                            else if (expectedType != null)
                                            {
                                                Debug.LogWarning($"Type mismatch: Expected {parameterType}, but got {expectedType} from assembly type name.");
                                            }
                                            else
                                            {
                                                Debug.LogWarning($"Could not resolve type: {assemblyTypeName}. Assigning object directly.");
                                                parameterValues[i] = unityObject;
                                            }
                                        }
                                        else
                                        {
                                            // Assign directly if no assembly type name is provided
                                            parameterValues[i] = unityObject;
                                        }
                                    }
                                    //else
                                    //{
                                    //    Debug.LogWarning($"Missing UnityEngine.Object for parameter at index {i}");
                                    //}
                                }
                                else
                                {
                                    Debug.LogWarning($"Unsupported parameter type: {parameterType}");
                                }
                            }
                            return parameterValues;
                        }
                    }
                }
            }

            return null;
        }


        public static void DrawSquare(Vector2 nodePosition, float squareSize, Color color)
        {
            // Draw the four lines to form a square
            Vector2 topLeft = nodePosition + new Vector2(-squareSize / 2, squareSize / 2);
            Vector2 topRight = nodePosition + new Vector2(squareSize / 2, squareSize / 2);
            Vector2 bottomLeft = nodePosition + new Vector2(-squareSize / 2, -squareSize / 2);
            Vector2 bottomRight = nodePosition + new Vector2(squareSize / 2, -squareSize / 2);

            // Drawing the square edges as lines
            Debug.DrawLine(topLeft, topRight, color);
            Debug.DrawLine(topRight, bottomRight, color);
            Debug.DrawLine(bottomRight, bottomLeft, color);
            Debug.DrawLine(bottomLeft, topLeft, color);
        }
    }
}