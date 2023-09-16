using UnityEngine;
using UnityEditor;
using ScriptableArchitecture.Core;

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
                menu.ShowAsContext();
            }

            EditorGUI.EndProperty();
        }
    }
}