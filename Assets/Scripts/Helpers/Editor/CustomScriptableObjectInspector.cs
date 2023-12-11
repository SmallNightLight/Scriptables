using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ScriptableObject), true)]
public class CustomScriptableObjectInspector : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();

        // Add a button to go back to the previous selection
        if (GUILayout.Button("Go to Previous Selection"))
        {
            if (FolderSelectionHelper.lastActiveObject != null)
            {
                Selection.activeObject = FolderSelectionHelper.lastActiveObject;
            }
        }
    }
}