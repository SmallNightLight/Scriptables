using UnityEngine;
using UnityEditor;
using ScriptableArchitecture.Core;
using System;

namespace ScriptableArchitecture.EditorScript
{
    [CustomPropertyDrawer(typeof(Reference<,>), true)]
    public class ReferenceDrawer : PropertyDrawer
    {
        bool foldoutOpen;

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

            // Draw foldout
            Rect foldoutRect = new Rect(position.x, position.y, 15f, position.height);
            foldoutOpen = EditorGUI.Foldout(foldoutRect, foldoutOpen, GUIContent.none);
            if (foldoutOpen && isVariable && variableProperty.objectReferenceValue != null)
            {
                // Additional fields for modifying ScriptableObject directly
                position.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, variableProperty, new GUIContent("ScriptableObject"));
            }

            EditorGUI.EndProperty();
        }
    }
}