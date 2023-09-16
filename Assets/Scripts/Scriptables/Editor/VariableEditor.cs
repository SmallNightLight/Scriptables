using UnityEngine;
using UnityEditor;
using ScriptableArchitecture.Core;

namespace ScriptableArchitecture.EditorScript
{
    [CustomEditor(typeof(Variable<>), true)]
    public class VariableEditor : Editor
    {
        private Variable _target;
        private SerializedProperty _valueProperty;
        private SerializedProperty _showVariableProp;
        private SerializedProperty _defaultValueProperty;

        private void OnEnable()
        {
            _target = target as Variable;
            if (target == null)
                return;

            _valueProperty = serializedObject.FindProperty("Value");
            _showVariableProp = serializedObject.FindProperty("Type");
            _defaultValueProperty = serializedObject.FindProperty("DefaultValue");
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
                return;

            serializedObject.Update();

            EditorGUILayout.PropertyField(_valueProperty);
            EditorGUILayout.PropertyField(_showVariableProp);

            if ((VariableType)_showVariableProp.enumValueIndex == VariableType.ResetOnGameStart)
            {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                EditorGUILayout.PropertyField(_defaultValueProperty);
                serializedObject.ApplyModifiedProperties();
            }

            if (GUI.changed)
                EditorUtility.SetDirty(_target);

            serializedObject.ApplyModifiedProperties();
        }
    }
}