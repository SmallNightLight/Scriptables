using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;
using ScriptableArchitecture.Core;

namespace ScriptableArchitecture.EditorScript
{
    [CustomEditor(typeof(GameEventBase), true)]
    public class GameEventEditorEmpty : Editor
    {
        private GameEventBase _target;

        private void OnEnable()
        {
            _target = (GameEventBase)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Debug Options", EditorStyles.boldLabel);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Raise Event"))
                _target.Raise();

            EditorGUILayout.EndVertical();
        }
    }

    [CustomEditor(typeof(GameEventBase<>), true)]
    public class GameEventEditorWithVariable : Editor
    {
        private SerializedProperty _debugValueProperty;
        private MethodInfo _raiseMethod;

        private void OnEnable()
        {
            _debugValueProperty = serializedObject.FindProperty("DebugValue");
            _raiseMethod = target.GetType().BaseType.GetMethod("Raise", BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            serializedObject.Update();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Debug Options", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_debugValueProperty);
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Raise Event"))
            {
                Type targetType = _debugValueProperty.serializedObject.targetObject.GetType();
                FieldInfo targetField = targetType.GetField("DebugValue", BindingFlags.Instance | BindingFlags.Public);
                object debugValue = targetField.GetValue(_debugValueProperty.serializedObject.targetObject);
                _raiseMethod.Invoke(target, new object[1] { debugValue });
            }

            EditorGUILayout.EndVertical();
        }
    }
}