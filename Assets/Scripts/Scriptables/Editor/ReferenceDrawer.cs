using UnityEngine;
using UnityEditor;
using ScriptableArchitecture.Core;
using System;

namespace ScriptableArchitecture.EditorScript
{
    [CustomPropertyDrawer(typeof(Reference<,>), true)]
    public class ReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty isVariableProperty = property.FindPropertyRelative("_isVariable");
            SerializedProperty variableProperty = property.FindPropertyRelative("_variable");
            SerializedProperty constantProperty = property.FindPropertyRelative("_constant");

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            Rect valueRect = new Rect(position.x, position.y, position.width - 20f, position.height);
            Rect buttonRect = new Rect(position.x + position.width - 18f, position.y, 18f, position.height);

            bool isVariable = isVariableProperty.boolValue;
            SerializedProperty valueProperty = isVariable ? variableProperty : constantProperty;
            EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);

            //Display a button to change the reference type
            if (GUI.Button(buttonRect, "..", EditorStyles.miniButton))
            {
                //Display a popup menu
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Constant"), !isVariable, () =>
                {
                    isVariableProperty.boolValue = false;
                    property.serializedObject.ApplyModifiedProperties();
                });

                menu.AddItem(new GUIContent("Variable"), isVariable, () =>
                {
                    isVariableProperty.boolValue = true;
                    property.serializedObject.ApplyModifiedProperties();
                });

                menu.AddItem(new GUIContent("Create New"), false, () =>
                {
                    string originalTypeName = variableProperty.type;
                    int start = originalTypeName.IndexOf("<") + 2;
                    int end = originalTypeName.LastIndexOf(">");
                    string variableTypeName = originalTypeName.Substring(start, end - start);

                    Type newType = Type.GetType($"ScriptableArchitecture.Data.{variableTypeName}, ScriptableAssembly.Data");
                    Variable newVariable = ScriptableObject.CreateInstance(newType) as Variable;

                    string path = EditorUtility.SaveFilePanel($"Create new {variableTypeName}", "Assets/Data", "NewVariable", "asset");

                    if (!string.IsNullOrEmpty(path))
                    {
                        path = "Assets" + path.Substring(Application.dataPath.Length);

                        AssetDatabase.CreateAsset(newVariable, path);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        
                        variableProperty.objectReferenceValue = newVariable;
                        isVariableProperty.boolValue = true;
                        property.serializedObject.ApplyModifiedProperties();
                    }
                });

                menu.ShowAsContext();
            }

            EditorGUI.EndProperty();
        }

        private Type GetTypeFromTypeName(string typeName)
        {
            // Handle Unity's specialized types
            if (typeName.StartsWith("PPtr<$") && typeName.EndsWith(">"))
            {
                // Extract the inner type name, e.g., "FloatVariable" from "PPtr<$FloatVariable>"
                string innerTypeName = typeName.Substring(6, typeName.Length - 7);
                // Construct the full type name assuming it's in the same namespace
                //string fullTypeName = $"{typeof(Reference<>).Namespace}.{innerTypeName}";
                return Type.GetType(innerTypeName);
            }
            else
            {
                // Default case: use Type.GetType directly
                return Type.GetType(typeName);
            }
        }
    }
}