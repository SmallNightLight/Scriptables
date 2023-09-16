using UnityEditor;
using UnityEngine;

namespace ScriptableArchitecture.EditorScript
{
    [CustomPropertyDrawer(typeof(Component), true)]
    public class TestEditor : PropertyDrawer
    {
        private GUIStyle outlineStyle;

        public TestEditor()
        {
            outlineStyle = new GUIStyle(GUI.skin.label);
            outlineStyle.normal.textColor = Color.white;
            outlineStyle.normal.background = MakeText(2, 2, new Color(1, 0, 0, 0.5f));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Debug.Log("GUI");
            EditorGUI.LabelField(position, label, outlineStyle);
            position.y += EditorGUIUtility.singleLineHeight;
            EditorGUI.PropertyField(position, property, GUIContent.none, true);
        }

        private Texture2D MakeText(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pixels);
            result.Apply();
            return result;
        }
    }
}