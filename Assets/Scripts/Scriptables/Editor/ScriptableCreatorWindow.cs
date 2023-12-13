using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assembly = System.Reflection.Assembly;

namespace ScriptableArchitecture.EditorScript
{
    public class ScriptableCreatorWindow : EditorWindow
    {
        private Assembly[] _assemblies;

        private int _currentToolbar = 0;
        private string[] _toolbarNames = { "Scriptable Creator", "Data Creator" };

        private enum WindowOptions { Empty, Content, Creator }
        private WindowOptions _currentDataWindow = WindowOptions.Empty;
        string _currentScriptName;
        string _currentScriptContents;

        //Scriptables variables
        private string _scriptableType;
        private Vector2 _scrollPositionScriptable;

        //Datapoint variables
        private string _dataPointName;
        private List<PropertyData> _dataPointProperties = new List<PropertyData>();
        private Vector2 _scrollPositionDataPointCreator;
        private Vector2 _scrollPositionDataPoints;

        #region Folder paths

        const string _dataPointsPath = "Assets/Scripts/Scriptables/Data/DataPoints";
        const string _eventListenersPath = "Assets/Scripts/Scriptables/Data/EventListeners";
        const string _gameEventsPath = "Assets/Scripts/Scriptables/Data/GameEvents";
        const string _referencesPath = "Assets/Scripts/Scriptables/Data/References";
        const string _variablesPath = "Assets/Scripts/Scriptables/Data/Variables";

        #endregion

        private void OnEnable()
        {
            LoadAssemblies();
        }

        [MenuItem("Window/Scriptable Creator")]
        public static void OpenWindow()
        {
            ScriptableCreatorWindow window = GetWindow<ScriptableCreatorWindow>();
            window.titleContent = new GUIContent("Scriptable Creator");
            window.Show();
        }

        private void OnGUI()
        {
            int newToolbar = GUILayout.Toolbar(_currentToolbar, _toolbarNames);
            
            if (newToolbar != _currentToolbar)
            {
                _currentScriptName = "";
                _currentScriptContents = "";

                _currentToolbar = newToolbar;
            }
            
            if (_currentToolbar == 0)
            {
                //Display Scriptable Creator Window

                EditorGUILayout.BeginHorizontal();

                GUIWindowList("Scriptables", _variablesPath, "New Scriptable", _scrollPositionScriptable, 11);

                switch (_currentDataWindow)
                {
                    case WindowOptions.Empty:
                        break;
                    case WindowOptions.Content:
                        GUIShowContents();
                        break;
                    case WindowOptions.Creator:
                        GUICreateScriptable();
                        break;
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                //Datapoint window

                EditorGUILayout.BeginHorizontal();

                GUIWindowList("DataPoints", _dataPointsPath, "New Datapoint", _scrollPositionDataPoints);

                switch (_currentDataWindow)
                {
                    case WindowOptions.Empty:
                        break;
                    case WindowOptions.Content:
                        GUIShowContents();
                        break;
                    case WindowOptions.Creator:
                        GUICreateDataPoint();
                        break;
                }

                EditorGUILayout.EndHorizontal();
            }

            //Handle control of the editor window
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                GUI.FocusControl(null);
                Repaint();
            }
        }

        private void GUIWindowList(string labelName, string folderPath, string createrButtonName, Vector2 scrollPosition, int fileNameExclude = 3)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(215));
            EditorGUILayout.Space();
            GUILayout.Label(labelName, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            string[] assetPaths = AssetDatabase.FindAssets("", new[] { folderPath }).Select(AssetDatabase.GUIDToAssetPath).ToArray();

            foreach (string assetPath in assetPaths)
            {
                try
                {
                    string fileName = Path.GetFileName(assetPath);
                    fileName = fileName.Substring(0, fileName.Length - fileNameExclude);

                    string fileContents = File.ReadAllText(assetPath);

                    if (GUILayout.Button(fileName, GUILayout.MaxWidth(200)))
                    {
                        _currentDataWindow = WindowOptions.Content;
                        _currentScriptName = fileName;
                        _currentScriptContents = fileContents;
                        Repaint();
                    }
                }
                catch
                {

                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button(createrButtonName))
                _currentDataWindow = WindowOptions.Creator;

            EditorGUILayout.EndVertical();
        }

        private void GUIShowContents()
        {
            if (string.IsNullOrEmpty(_currentScriptName) || string.IsNullOrEmpty(_currentScriptContents))
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space();
            GUILayout.Label(_currentScriptName, EditorStyles.boldLabel);
            
            if (GUILayout.Button("Remove"))
            {
                RemoveDataPoint(_currentScriptName);
            }

            EditorGUILayout.Space();
            GUILayout.Label(_currentScriptContents);
            GUILayout.EndVertical();
        }

        private void RemoveDataPoint(string baseName)
        {
            File.Delete($"{_dataPointsPath}/{baseName}.cs");
            AssetDatabase.Refresh();
        }

        private void RemoveScriptables(string baseName)
        {

        }

        private void GUICreateScriptable()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space();
            _scriptableType = EditorGUILayout.TextField("Scriptable type:", _scriptableType);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Scriptable", GUILayout.Width(20)))
                CreateScriptable();

            GUILayout.EndVertical();
        }

        private void GUICreateDataPoint()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.Space();
            _dataPointName = EditorGUILayout.TextField("Datapoint name:", _dataPointName);
            EditorGUILayout.Space();
            GUILayout.Label("Properties", EditorStyles.boldLabel);

            _scrollPositionDataPointCreator = EditorGUILayout.BeginScrollView(_scrollPositionDataPointCreator);

            for (int i = 0; i < _dataPointProperties.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label("Property Name:", GUILayout.Width(100));
                _dataPointProperties[i].propertyName = EditorGUILayout.TextField(_dataPointProperties[i].propertyName);

                GUILayout.Label("Property Type:", GUILayout.Width(100));
                _dataPointProperties[i].propertyType = EditorGUILayout.TextField(_dataPointProperties[i].propertyType);

                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    _dataPointProperties.RemoveAt(i);
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add Property", GUILayout.Width(100)))
                _dataPointProperties.Add(new PropertyData());

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space();

            //Create Datapoint Button
            if (GUILayout.Button("Create Datapoint"))
                CreateDataPoint();

            EditorGUILayout.EndVertical();
        }

        private void CreateScriptable()
        {
            if (string.IsNullOrEmpty(_scriptableType))
            {
                Debug.LogWarning("No scriptable type selected!");
                return;
            }

            string baseScript = "";
            string nameSpace = GetNameSpace(_scriptableType);

            if (nameSpace != null)
                baseScript += $"using {nameSpace};\n";
            else
            {
                //Check if variable is non-primitive
                if (ConvertToSystemType(_scriptableType) == null)
                {
                    Debug.LogWarning($"NameSpace of property type not found: {_scriptableType}");
                    return;
                }
            }

            string scriptName = _scriptableType.CapitalizeFirstLetter();

            CreateScript(_variablesPath, scriptName + "Variable", GetVariableScript(_scriptableType, scriptName, baseScript));
            CreateScript(_referencesPath, scriptName + "Reference", GetReferenceScript(_scriptableType, scriptName, baseScript));
            CreateScript(_gameEventsPath, scriptName + "GameEvent", GetGameEventScript(_scriptableType, scriptName, baseScript));
            CreateScript(_eventListenersPath, scriptName + "GameEventListener", GetGameEventListenerScript(_scriptableType, scriptName, baseScript));

            _currentDataWindow = WindowOptions.Content;
            _currentScriptName = _scriptableType.CapitalizeFirstLetter();
            _currentScriptContents = "";
            Repaint();

            _scriptableType = "";
            _scrollPositionScriptable = Vector2.zero;
        }

        private void CreateScript(string folderPath, string scriptName, string script)
        {
            string scriptFilePath = $"{folderPath}/{scriptName}.cs";

            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets", folderPath.Replace("Assets/", ""));

            File.WriteAllText(scriptFilePath, script);
            AssetDatabase.Refresh();
        }

        private void CreateDataPoint()
        {
            if (string.IsNullOrEmpty(_dataPointName))
            {
                Debug.LogWarning("No script name selected!");
                return;
            }

            //Check if all properties have a value
            foreach (PropertyData property in _dataPointProperties)
            {
                if (string.IsNullOrEmpty(property.propertyType) || string.IsNullOrEmpty(property.propertyName))
                {
                    Debug.LogWarning("A property is not correctly defined");
                    return;
                }
            }

            string scriptTemplate = "using ScriptableArchitecture.Core;\n";

            List<string> addedNameSpaces = new List<string>
            {
                "ScriptableArchitecture.Core"
            };

            //Add NameSpaces
            for (int i = _dataPointProperties.Count - 1; i >= 0; i--)
            {
                string nameSpace = GetNameSpace(_dataPointProperties[i].propertyType);
                if (nameSpace != null)
                {
                    if (!addedNameSpaces.Contains(nameSpace))
                    {
                        addedNameSpaces.Add(nameSpace);
                        scriptTemplate += $"using {nameSpace};\n";
                    }
                }
                else
                {
                    if (ConvertToSystemType(_dataPointProperties[i].propertyType) != null)
                    {
                        //Primitive variable type
                        continue;
                    }
                    else
                    {
                        Debug.LogWarning($"NameSpace of property type not found: {_dataPointProperties[i].propertyType}");
                        return;
                    }
                }
            }

            if (addedNameSpaces.Count != 0)
                scriptTemplate += "\n";

            scriptTemplate += $@"namespace ScriptableArchitecture.Data
{{
    [System.Serializable]
    public struct {_dataPointName} : IDataPoint" + "\n    {\n";

            //Add properties
            foreach (PropertyData property in _dataPointProperties)
                scriptTemplate += $"        public {property.propertyType} {property.propertyName};\n";

            scriptTemplate += "    }\n}";

            string scriptFilePath = $"{_dataPointsPath}/{_dataPointName}.cs";

            if (!AssetDatabase.IsValidFolder(_dataPointsPath))
                AssetDatabase.CreateFolder("Assets", _dataPointsPath.Replace("Assets/", "")); 

            if (AssetDatabase.LoadAssetAtPath(scriptFilePath, typeof(UnityEngine.Object)) != null)
            {
                Debug.LogWarning("A script with that name already exists!");
                return;
            }

            File.WriteAllText(scriptFilePath, scriptTemplate);
            AssetDatabase.Refresh();

            _currentDataWindow = WindowOptions.Content;
            _currentScriptName = _dataPointName;
            _currentScriptContents = scriptTemplate;
            Repaint();

            _dataPointName = "";
            _dataPointProperties.Clear();
            _scrollPositionDataPointCreator = Vector2.zero;
        }

        private string GetNameSpace(string typeName)
        {
            foreach (Assembly assembly in _assemblies)
            {
                string namespaceName = assembly.GetTypes().Where(t => t.Name == typeName).Select(t => t.Namespace).FirstOrDefault();
                if (!string.IsNullOrEmpty(namespaceName))
                    return namespaceName;
            }

            return null;
        }

        private void LoadAssemblies()
        {
            _assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }

        public static Type ConvertToSystemType(string typeName)
        {
            return typeName switch
            {
                "bool" => typeof(bool),
                "byte" => typeof(byte),
                "sbyte" => typeof(sbyte),
                "char" => typeof(char),
                "decimal" => typeof(decimal),
                "double" => typeof(double),
                "float" => typeof(float),
                "int" => typeof(int),
                "uint" => typeof(uint),
                "nint" => typeof(IntPtr),
                "nuint" => typeof(UIntPtr),
                "long" => typeof(long),
                "ulong" => typeof(ulong),
                "short" => typeof(short),
                "ushort" => typeof(ushort),
                "object" => typeof(object),
                "string" => typeof(string),
                "dynamic" => typeof(object),
                _ => null //Invalid type keyword
            };
        }

        private string GetVariableScript(string type, string scriptName, string baseScript)
        {
            if (baseScript.Contains("using UnityEngine;") || baseScript.Contains("using ScriptableArchitecture.Data;"))
                baseScript = "";

            return baseScript + $@"using ScriptableArchitecture.Core;
using UnityEngine;

namespace ScriptableArchitecture.Data
{{
    [CreateAssetMenu(fileName = ""{scriptName}Variable"", menuName = ""Scriptables/Variables/{scriptName}"")]
    public class {scriptName}Variable : Variable<{type}>
    {{
    }}
}}";
        }

        private string GetReferenceScript(string type, string scriptName, string baseScript)
        {
            if (baseScript.Contains("using ScriptableArchitecture.Data;"))
                baseScript = "";

            if (baseScript != "")
                baseScript += "\n";

            return baseScript + $@"using ScriptableArchitecture.Core;

namespace ScriptableArchitecture.Data
{{
    [System.Serializable]
    public class {scriptName}Reference : Reference<{type}, {scriptName}Variable>
    {{
    }}
}}";
        }

        private string GetGameEventScript(string type, string scriptName, string baseScript)
        {
            if (baseScript.Contains("using UnityEngine;") || baseScript.Contains("using ScriptableArchitecture.Data;"))
                baseScript = "";

            return baseScript + $@"using ScriptableArchitecture.Core;
using UnityEngine;

namespace ScriptableArchitecture.Data
{{
    [CreateAssetMenu(fileName = ""{scriptName}GameEvent"", menuName = ""Scriptables/GameEvents/{scriptName}GameEvent"")]
    public class {scriptName}GameEvent : GameEventBase<{type}>
    {{
    }}
}}";

        }

        private string GetGameEventListenerScript(string type, string scriptName, string baseScript)
        {
            if (baseScript.Contains("using UnityEngine;") || baseScript.Contains("using ScriptableArchitecture.Data;"))
                baseScript = "";

            return baseScript + $@"using ScriptableArchitecture.Core;
using UnityEngine;

namespace ScriptableArchitecture.Data
{{
    [AddComponentMenu(""GameEvent Listeners/{scriptName} Event Listener"")]
    public class {scriptName}GameEventListener : GameEventListenerBase<{type}>
    {{
    }}
}}";
        }

        private class PropertyData
        {
            public string propertyName;
            public string propertyType;
        }
    }

    public static class StringExtensions
    {
        public static string CapitalizeFirstLetter(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return char.ToUpper(input[0]) + input.Substring(1);
        }
    }
}