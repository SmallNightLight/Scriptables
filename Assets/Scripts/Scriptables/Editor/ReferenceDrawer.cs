using ScriptableArchitecture.Core;
using System;
using UnityEditor;
using UnityEngine;

namespace ScriptableArchitecture.EditorScript
{
    [CustomPropertyDrawer(typeof(Reference<,>), true)]
    public class ReferenceDrawer : PropertyDrawer
    {
        bool foldoutOpen;
        bool isVariable;
        float height = 18;
        bool expandedValue;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty isVariableProperty = property.FindPropertyRelative("_isVariable");
            SerializedProperty variableProperty = property.FindPropertyRelative("_variable");
            SerializedProperty constantProperty = property.FindPropertyRelative("_constant");

            isVariable = isVariableProperty.boolValue;
            
            height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (isVariable)
            {
                if (variableProperty.boxedValue != null)
                {
                    position.x += 15f;
                    position.width += 15;

                    position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                    position.x -= 15f;

                    Rect variableRect = new Rect(position.x, position.y, position.width - 35f, EditorGUIUtility.singleLineHeight);
                    
                    EditorGUI.PropertyField(variableRect, variableProperty, GUIContent.none);
                    
                    Rect foldoutRect = new Rect(0, 0, 15f, EditorGUIUtility.singleLineHeight);
                    
                    //Draw foldout
                    foldoutOpen = EditorGUI.Foldout(foldoutRect, foldoutOpen, GUIContent.none);
                    if (foldoutOpen && isVariable && variableProperty.objectReferenceValue != null)
                    {
                        Rect valueRect = new Rect(0, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing, EditorGUIUtility.currentViewWidth - 20, EditorGUIUtility.singleLineHeight);
                        EditorGUI.indentLevel++;

                        var valueVariable = variableProperty.objectReferenceValue as Variable;
                        SerializedObject serializedObject = new SerializedObject(valueVariable);
                        SerializedProperty valueProperty = serializedObject.FindProperty("Value");
                        
                        EditorGUI.BeginChangeCheck();

                        valueProperty.isExpanded = expandedValue;
                        EditorGUI.PropertyField(valueRect, valueProperty, true);
                        expandedValue = valueProperty.isExpanded;

                        if (EditorGUI.EndChangeCheck())
                            serializedObject.ApplyModifiedProperties();

                        height = EditorGUI.GetPropertyHeight(valueProperty) + EditorGUIUtility.standardVerticalSpacing + 2;
                        EditorGUI.indentLevel--;
                    }

                    position.width -= 15;
                }
                else
                {
                    position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                    Rect valueRect = new Rect(position.x, position.y, position.width - 72f, position.height);

                    EditorGUI.PropertyField(valueRect, variableProperty, GUIContent.none);

                    Rect newRect = new Rect(position.x + position.width - 70f, position.y, 50f, position.height);
                    if (GUI.Button(newRect, "New", EditorStyles.miniButton))
                    {
                        Type newType = GetVariableType(variableProperty.type, out string variableTypeName);
                        Variable newVariable = ScriptableObject.CreateInstance(newType) as Variable;

                        string path = EditorUtility.SaveFilePanel($"Create new {variableTypeName}", "Assets/Data", variableTypeName, "asset");

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
                    }
                }
            }
            else
            {
                Rect valueRect = new Rect(0, 0, position.width - 20f, position.height);
                EditorGUI.PropertyField(valueRect, constantProperty, new GUIContent(property.displayName), true);
                height = constantProperty.isExpanded ? EditorGUI.GetPropertyHeight(constantProperty, GUIContent.none, true) - EditorGUIUtility.singleLineHeight : 0;
            }

            Rect buttonRect = new Rect(position.x + position.width - 18f, position.y, 18f, position.height);

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

                menu.ShowAsContext();
            }

            EditorGUI.EndProperty();
        }

        private Type GetVariableType(string name, out string variableTypeName)
        {
            int start = name.IndexOf("<") + 2;
            int end = name.LastIndexOf(">");

            variableTypeName = name.Substring(start, end - start);
            return Type.GetType($"ScriptableArchitecture.Data.{variableTypeName}, ScriptableAssembly.Data");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float baseHeight = base.GetPropertyHeight(property, label);

            if ((foldoutOpen && isVariable) || !isVariable)
                return baseHeight + height;

            return baseHeight;
        }
    }
}