//Extended for Event Property - trying to display multiple input fields in the editor
//To do:
//- use a dict to store listeners and a way to change their proeprties
//- display multiple fiels
//- drag gameobjects to create a new unityevent
/**/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEditorInternal;

[CustomPropertyDrawer(typeof(UnityEventBase), true)]
public class EventExtensions : PropertyDrawer
{
    protected class State
    {
        internal ReorderableList m_ReorderableList;
        public int lastSelectedIndex;
    }

    private struct ValidMethodMap
    {
        public UnityEngine.Object m_target;
        public MethodInfo m_methodInfo;
        public PersistentListenerMode[] m_modes;

        public ValidMethodMap(UnityEngine.Object target, MethodInfo methodInfo, PersistentListenerMode[] modes)
        {
            m_target = target;
            m_methodInfo = methodInfo;
            m_modes = modes;
        }
    }

    private struct UnityEventFunction
    {
        private readonly SerializedProperty m_Listener;
        private readonly ValidMethodMap m_validMethodMap;

        public UnityEventFunction(SerializedProperty listener, ValidMethodMap validMethodMap)
        {
            m_Listener = listener;
            m_validMethodMap = validMethodMap;
        }

        public void Assign()
        {
            SerializedProperty serializedProperty = m_Listener.FindPropertyRelative("m_Target");
            SerializedProperty serializedProperty2 = m_Listener.FindPropertyRelative("m_TargetAssemblyTypeName");
            SerializedProperty serializedProperty3 = m_Listener.FindPropertyRelative("m_MethodName");
            SerializedProperty serializedProperty4 = m_Listener.FindPropertyRelative("m_Mode");
            SerializedProperty serializedProperty5 = m_Listener.FindPropertyRelative("m_Arguments");
            serializedProperty.objectReferenceValue = m_validMethodMap.m_target;
            serializedProperty2.stringValue = m_validMethodMap.m_methodInfo.DeclaringType.AssemblyQualifiedName;
            serializedProperty3.stringValue = m_validMethodMap.m_methodInfo.Name;
            serializedProperty4.enumValueIndex = (int)m_validMethodMap.m_modes[0]; //not only 0

            if (m_validMethodMap.m_modes[0] == PersistentListenerMode.Object) //propably wrong
            {
                SerializedProperty serializedProperty6 = serializedProperty5.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
                ParameterInfo[] parameters = m_validMethodMap.m_methodInfo.GetParameters();
                if (parameters.Length == 1 && typeof(UnityEngine.Object).IsAssignableFrom(parameters[0].ParameterType))
                {
                    serializedProperty6.stringValue = parameters[0].ParameterType.AssemblyQualifiedName;
                }
                else
                {
                    serializedProperty6.stringValue = typeof(UnityEngine.Object).AssemblyQualifiedName;
                }
            }

            //Looping possible?
            foreach(PersistentListenerMode mode in m_validMethodMap.m_modes)
                ValidateObjectParamater(serializedProperty5, mode);
            m_Listener.serializedObject.ApplyModifiedProperties();
        }

        private void ValidateObjectParamater(SerializedProperty arguments, PersistentListenerMode mode)
        {
            SerializedProperty serializedProperty = arguments.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
            SerializedProperty serializedProperty2 = arguments.FindPropertyRelative("m_ObjectArgument");
            UnityEngine.Object objectReferenceValue = serializedProperty2.objectReferenceValue;
            if (mode != PersistentListenerMode.Object)
            {
                serializedProperty.stringValue = typeof(UnityEngine.Object).AssemblyQualifiedName;
                serializedProperty2.objectReferenceValue = null;
            }
            else if (!(objectReferenceValue == null))
            {
                Type type = Type.GetType(serializedProperty.stringValue, throwOnError: false);
                if (!typeof(UnityEngine.Object).IsAssignableFrom(type) || !type.IsInstanceOfType(objectReferenceValue))
                {
                    serializedProperty2.objectReferenceValue = null;
                }
            }
        }

        public void Clear()
        {
            SerializedProperty serializedProperty = m_Listener.FindPropertyRelative("m_MethodName");
            serializedProperty.stringValue = null;
            SerializedProperty serializedProperty2 = m_Listener.FindPropertyRelative("m_Mode");
            serializedProperty2.enumValueIndex = 1;
            m_Listener.serializedObject.ApplyModifiedProperties();
        }
    }

    private Dictionary<SerializedProperty, Type[]> m_listenerParameters;

    private const string kNoFunctionString = "No Function";

    internal const string kInstancePath = "m_Target";

    internal const string kInstanceTypePath = "m_TargetAssemblyTypeName";

    internal const string kCallStatePath = "m_CallState";

    internal const string kArgumentsPath = "m_Arguments";

    internal const string kModePath = "m_Mode";

    internal const string kMethodNamePath = "m_MethodName";

    internal const string kFloatArgument = "m_FloatArgument";

    internal const string kIntArgument = "m_IntArgument";

    internal const string kObjectArgument = "m_ObjectArgument";

    internal const string kStringArgument = "m_StringArgument";

    internal const string kBoolArgument = "m_BoolArgument";

    internal const string kObjectArgumentAssemblyTypeName = "m_ObjectArgumentAssemblyTypeName";

    private const string kDotString = ".";

    private const string kArrayDataString = "Array.data[";

    private static readonly char[] kDotSeparator = new char[1] { '.' };

    private static readonly char[] kClosingSquareBraceSeparator = new char[1] { ']' };

    private string m_Text;

    private UnityEventBase m_DummyEvent;

    private SerializedProperty m_Prop;

    private SerializedProperty m_ListenersArray;

    private const int kExtraSpacing = 9;

    private ReorderableList m_ReorderableList;

    private int m_LastSelectedIndex;

    private Dictionary<string, State> m_States = new Dictionary<string, State>();

    private static string GetEventParams(UnityEventBase evt)
    {
        Type[] eventTypes = evt.GetType().GetGenericArguments();

        if (eventTypes.Length == 0)
            return "";

        string paramList = " <";

        for (int i = 0; i < eventTypes.Length; i++)
        {
            paramList += eventTypes[i].Name;

            if (i < eventTypes.Length - 1)
                paramList += ", ";
        }

        return paramList + ">";
    }

    private State GetState(SerializedProperty prop)
    {
        string propertyPath = prop.propertyPath;
        m_States.TryGetValue(propertyPath, out var value);
        if (value == null || value.m_ReorderableList.serializedProperty.serializedObject != prop.serializedObject)
        {
            if (value == null)
            {
                value = new State();
            }

            SerializedProperty elements = prop.FindPropertyRelative("m_PersistentCalls.m_Calls");
            value.m_ReorderableList = new ReorderableList(prop.serializedObject, elements, draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true)
            {
                drawHeaderCallback = DrawEventHeader,
                drawElementCallback = DrawEvent,
                onSelectCallback = OnSelectEvent,
                onReorderCallback = OnReorderEvent,
                onAddCallback = OnAddEvent,
                onRemoveCallback = OnRemoveEvent
            };
            SetupReorderableList(value.m_ReorderableList);
            m_States[propertyPath] = value;
        }

        return value;
    }

    private State RestoreState(SerializedProperty property)
    {
        State state = GetState(property);
        m_ListenersArray = state.m_ReorderableList.serializedProperty;
        m_ReorderableList = state.m_ReorderableList;
        m_LastSelectedIndex = state.lastSelectedIndex;
        m_ReorderableList.index = m_LastSelectedIndex;
        return state;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        m_Prop = property;
        m_Text = label.text;
        State state = RestoreState(property);
        OnGUI(position);
        state.lastSelectedIndex = m_LastSelectedIndex;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        RestoreState(property);
        float result = 0f;
        if (m_ReorderableList != null)
        {
            result = m_ReorderableList.GetHeight();
        }

        return result;
    }

    public void OnGUI(Rect position)
    {
        if (m_ListenersArray != null && m_ListenersArray.isArray)
        {
            m_DummyEvent = GetDummyEvent(m_Prop);
            if (m_DummyEvent != null && m_ReorderableList != null)
            {
                int indentLevel = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                m_ReorderableList.DoList(position);
                EditorGUI.indentLevel = indentLevel;
            }
        }
    }

    protected virtual void SetupReorderableList(ReorderableList list)
    {
        list.elementHeight = 36f + 2f + 9f;
    }

    protected virtual void DrawEventHeader(Rect headerRect)
    {
        headerRect.height = 18f;
        string text = (string.IsNullOrEmpty(m_Text) ? "Event" : m_Text) + GetEventParams(m_DummyEvent);
        GUI.Label(headerRect, text);
    }

    private static PersistentListenerMode GetMode(SerializedProperty mode)
    {
        return (PersistentListenerMode)mode.enumValueIndex;
    }

    protected virtual void DrawEvent(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty arrayElementAtIndex = m_ListenersArray.GetArrayElementAtIndex(index);
        rect.y++;
        Rect[] rowRects = GetRowRects(rect);
        Rect position = rowRects[0];
        Rect position2 = rowRects[1];
        Rect rect2 = rowRects[2];
        Rect position3 = rowRects[3];
        SerializedProperty property = arrayElementAtIndex.FindPropertyRelative("m_CallState");
        SerializedProperty mode = arrayElementAtIndex.FindPropertyRelative("m_Mode");
        SerializedProperty serializedProperty = arrayElementAtIndex.FindPropertyRelative("m_Arguments");
        SerializedProperty serializedProperty2 = arrayElementAtIndex.FindPropertyRelative("m_Target");
        SerializedProperty serializedProperty3 = arrayElementAtIndex.FindPropertyRelative("m_MethodName");
        Color backgroundColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.white;
        EditorGUI.PropertyField(position, property, GUIContent.none);
        EditorGUI.BeginChangeCheck();
        GUI.Box(position2, GUIContent.none);
        EditorGUI.PropertyField(position2, serializedProperty2, GUIContent.none);
        if (EditorGUI.EndChangeCheck())
        {
            serializedProperty3.stringValue = null;
        }

        PersistentListenerMode persistentListenerMode = GetMode(mode);
        if (serializedProperty2.objectReferenceValue == null || string.IsNullOrEmpty(serializedProperty3.stringValue))
        {
            persistentListenerMode = PersistentListenerMode.Void;
        }

        SerializedProperty serializedProperty4 = persistentListenerMode switch
        {
            PersistentListenerMode.Float => serializedProperty.FindPropertyRelative("m_FloatArgument"),
            PersistentListenerMode.Int => serializedProperty.FindPropertyRelative("m_IntArgument"),
            PersistentListenerMode.Object => serializedProperty.FindPropertyRelative("m_ObjectArgument"),
            PersistentListenerMode.String => serializedProperty.FindPropertyRelative("m_StringArgument"),
            PersistentListenerMode.Bool => serializedProperty.FindPropertyRelative("m_BoolArgument"),
            _ => serializedProperty.FindPropertyRelative("m_IntArgument"),
        };
        string stringValue = serializedProperty.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName").stringValue;
        Type type = typeof(UnityEngine.Object);
        if (!string.IsNullOrEmpty(stringValue))
        {
            type = Type.GetType(stringValue, throwOnError: false) ?? typeof(UnityEngine.Object);
        }

        //need change here?
        if (persistentListenerMode == PersistentListenerMode.Object)
        {
            EditorGUI.BeginChangeCheck();
            UnityEngine.Object objectReferenceValue = EditorGUI.ObjectField(position3, GUIContent.none, serializedProperty4.objectReferenceValue, type, allowSceneObjects: true);
            if (EditorGUI.EndChangeCheck())
            {
                serializedProperty4.objectReferenceValue = objectReferenceValue;
            }
        }
        else if (persistentListenerMode != PersistentListenerMode.Void && persistentListenerMode != 0)
        {
            //Draw here
            Rect pos1, pos2;
            pos1 = pos2 = position3;
            pos2.width = pos1.width /= 2;
            pos2.x += pos2.width;

            EditorGUI.PropertyField(pos1, serializedProperty4, GUIContent.none);
            EditorGUI.PropertyField(pos2, serializedProperty.FindPropertyRelative("m_StringArgument"), GUIContent.none);
        }

        using (new EditorGUI.DisabledScope(serializedProperty2.objectReferenceValue == null))
        {
            EditorGUI.BeginProperty(rect2, GUIContent.none, serializedProperty3);
            GUIContent content = new GUIContent("ERROR");
            if (EditorGUI.showMixedValue)
            {
                //content = EditorGUI.mixedValueContent;
                Debug.Log("Not implemented mixed values");
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                if (serializedProperty2.objectReferenceValue == null || string.IsNullOrEmpty(serializedProperty3.stringValue))
                {
                    stringBuilder.Append("No Function");
                }
                else if (!IsPersistantListenerValid(m_DummyEvent, serializedProperty3.stringValue, serializedProperty2.objectReferenceValue, GetMode(mode), type))
                {
                    string arg = "UnknownComponent";
                    UnityEngine.Object objectReferenceValue2 = serializedProperty2.objectReferenceValue;
                    if (objectReferenceValue2 != null)
                    {
                        arg = objectReferenceValue2.GetType().Name;
                    }

                    stringBuilder.Append($"<Missing {arg}.{serializedProperty3.stringValue}>");
                }
                else
                {
                    stringBuilder.Append(serializedProperty2.objectReferenceValue.GetType().Name);
                    if (!string.IsNullOrEmpty(serializedProperty3.stringValue))
                    {
                        stringBuilder.Append(".");
                        if (serializedProperty3.stringValue.StartsWith("set_"))
                        {
                            stringBuilder.Append(serializedProperty3.stringValue.Substring(4));
                        }
                        else
                        {
                            stringBuilder.Append(serializedProperty3.stringValue);
                        }
                    }
                }

               content = new GUIContent(stringBuilder.ToString());
            }

            if (EditorGUI.DropdownButton(rect2, content, FocusType.Passive, EditorStyles.popup))
            {
                BuildPopupList(serializedProperty2.objectReferenceValue, m_DummyEvent, arrayElementAtIndex).DropDown(rect2);
            }

            EditorGUI.EndProperty();
        }

        GUI.backgroundColor = backgroundColor;
    }

    private Rect[] GetRowRects(Rect rect)
    {
        Rect[] array = new Rect[4];
        rect.height = 18f;
        rect.y += 2f;
        Rect rect2 = rect;
        rect2.width *= 0.3f;
        Rect rect3 = rect2;
        rect3.y += EditorGUIUtility.singleLineHeight +2;
        Rect rect4 = rect;
        rect4.xMin = rect3.xMax + 5f;
        Rect rect5 = rect4;
        rect5.y += EditorGUIUtility.singleLineHeight + 2;
        array[0] = rect2;
        array[1] = rect3;
        array[2] = rect4;
        array[3] = rect5;
        return array;
    }

    protected virtual void OnRemoveEvent(ReorderableList list)
    {
        ReorderableList.defaultBehaviours.DoRemoveButton(list);
        m_LastSelectedIndex = list.index;
    }

    protected virtual void OnAddEvent(ReorderableList list)
    {
        if (m_ListenersArray.hasMultipleDifferentValues)
        {
            UnityEngine.Object[] targetObjects = m_ListenersArray.serializedObject.targetObjects;
            foreach (UnityEngine.Object obj in targetObjects)
            {
                using SerializedObject serializedObject = new SerializedObject(obj);
                serializedObject.FindProperty(m_ListenersArray.propertyPath).arraySize++;
                serializedObject.ApplyModifiedProperties();
            }

            m_ListenersArray.serializedObject.SetIsDifferentCacheDirty();
            m_ListenersArray.serializedObject.Update();
            list.index = list.serializedProperty.arraySize - 1;
        }
        else
        {
            ReorderableList.defaultBehaviours.DoAddButton(list);
        }

        m_LastSelectedIndex = list.index;
        SerializedProperty arrayElementAtIndex = m_ListenersArray.GetArrayElementAtIndex(list.index);
        SerializedProperty serializedProperty = arrayElementAtIndex.FindPropertyRelative("m_CallState");
        SerializedProperty serializedProperty2 = arrayElementAtIndex.FindPropertyRelative("m_Target");
        SerializedProperty serializedProperty3 = arrayElementAtIndex.FindPropertyRelative("m_MethodName");
        SerializedProperty serializedProperty4 = arrayElementAtIndex.FindPropertyRelative("m_Mode");
        SerializedProperty serializedProperty5 = arrayElementAtIndex.FindPropertyRelative("m_Arguments");
        serializedProperty.enumValueIndex = 2;
        serializedProperty2.objectReferenceValue = null;
        serializedProperty3.stringValue = null;
        serializedProperty4.enumValueIndex = 1;
        serializedProperty5.FindPropertyRelative("m_FloatArgument").floatValue = 0f;
        serializedProperty5.FindPropertyRelative("m_IntArgument").intValue = 0;
        serializedProperty5.FindPropertyRelative("m_ObjectArgument").objectReferenceValue = null;
        serializedProperty5.FindPropertyRelative("m_StringArgument").stringValue = null;
        serializedProperty5.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName").stringValue = null;
    }

    protected virtual void OnSelectEvent(ReorderableList list)
    {
        m_LastSelectedIndex = list.index;
    }

    protected virtual void OnReorderEvent(ReorderableList list)
    {
        m_LastSelectedIndex = list.index;
    }

    internal static UnityEventBase GetDummyEvent(SerializedProperty prop)
    {
        UnityEngine.Object targetObject = prop.serializedObject.targetObject;
        if (targetObject == null)
        {
            return new UnityEvent();
        }

        // Get the field from the serialized property
        FieldInfo field = targetObject.GetType().GetField(prop.name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (field != null)
        {
            // Get the field's type
            Type fieldType = field.FieldType;

            if (fieldType.IsSubclassOf(typeof(UnityEventBase)))
            {
                // Create an instance of the field's type
                return Activator.CreateInstance(fieldType) as UnityEventBase;
            }
        }

        return new UnityEvent();
    }

    private static IEnumerable<ValidMethodMap> CalculateMethodMap(UnityEngine.Object target, Type[] t, bool allowSubclasses)
    {
        List<ValidMethodMap> list = new List<ValidMethodMap>();
        if (target == null || t == null)
        {
            return list;
        }

        Type type = target.GetType();
        List<MethodInfo> list2 = (from x in type.GetMethods()
                                  where !x.IsSpecialName
                                  select x).ToList();
        IEnumerable<PropertyInfo> source = type.GetProperties().AsEnumerable();
        source = source.Where((PropertyInfo x) => x.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: true).Length == 0 && x.GetSetMethod() != null);
        list2.AddRange(source.Select((PropertyInfo x) => x.GetSetMethod()));
        foreach (MethodInfo item2 in list2)
        {
            ParameterInfo[] parameters = item2.GetParameters();
            PersistentListenerMode[] modes = new PersistentListenerMode[parameters.Length];

            if (parameters.Length != t.Length || item2.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: true).Length != 0 || item2.ReturnType != typeof(void))
            {
                continue;
            }

            bool flag = true;
            for (int i = 0; i < t.Length; i++)
            {
                if (!parameters[i].ParameterType.IsAssignableFrom(t[i]))
                {
                    flag = false;
                }

                if (allowSubclasses && t[i].IsAssignableFrom(parameters[i].ParameterType))
                {
                    flag = true;
                }

                if (flag)
                    modes[i] = GetModeFromType(t[i]); //Propably wrong 
            }

            if (flag)
                list.Add(new ValidMethodMap(target, item2, modes));
        }

        return list;
    }

    private static Type[] _availableTypes = { typeof(float), typeof(int), typeof(bool), typeof(string), typeof(UnityEngine.Object)};

    private static IEnumerable<ValidMethodMap> CalculateMethodMap(UnityEngine.Object target)
    {
        List<ValidMethodMap> list = new List<ValidMethodMap>();

        if (target == null)
            return list;

        Type type = target.GetType();
        List<MethodInfo> list2 = (from x in type.GetMethods()
                                  where !x.IsSpecialName
                                  select x).ToList();
        IEnumerable<PropertyInfo> source = type.GetProperties().AsEnumerable();
        source = source.Where((PropertyInfo x) => x.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: true).Length == 0 && x.GetSetMethod() != null);
        list2.AddRange(source.Select((PropertyInfo x) => x.GetSetMethod()));
        foreach (MethodInfo item2 in list2)
        {
            ParameterInfo[] parameters = item2.GetParameters();
            PersistentListenerMode[] modes = new PersistentListenerMode[parameters.Length];

            if (item2.GetCustomAttributes(typeof(ObsoleteAttribute), inherit: true).Length != 0 || item2.ReturnType != typeof(void))
                continue;

            int flagCount = 0;
            for (int i = 0; i < parameters.Length; i++)
            {
                foreach(Type t in _availableTypes)
                {
                    if (parameters[i].ParameterType.IsAssignableFrom(t) || t == typeof(UnityEngine.Object) && t.IsAssignableFrom(parameters[i].ParameterType))
                    {
                        flagCount++;
                        modes[i] = GetModeFromType(t);
                        break;
                    }
                }
            }

            if (flagCount == parameters.Length)
                list.Add(new ValidMethodMap(target, item2, modes));
        }

        return list;
    }

    private static PersistentListenerMode GetModeFromType(Type type)
    {
        return type switch
        {
            Type t when t == typeof(float) => PersistentListenerMode.Float,
            Type t when t == typeof(int) => PersistentListenerMode.Int,
            Type t when t == typeof(string) => PersistentListenerMode.String,
            Type t when t == typeof(bool) => PersistentListenerMode.Bool,
            Type t when t == typeof(UnityEngine.Object) => PersistentListenerMode.Object,
            _ => PersistentListenerMode.Int,
        };
    }

    public static bool IsPersistantListenerValid(UnityEventBase dummyEvent, string methodName, UnityEngine.Object uObject, PersistentListenerMode modeEnum, Type argumentType)
    {
        if (uObject == null || string.IsNullOrEmpty(methodName))
        {
            return false;
        }

        return FindMethod(uObject.GetType(), methodName, modeEnum, argumentType) != null;
    }

    private static MethodInfo FindMethod(Type listenerType, string name, PersistentListenerMode mode, Type argumentType)
    {
        return mode switch
        {
            PersistentListenerMode.EventDefined => UnityEventBase.GetValidMethodInfo(listenerType, name, new Type[0]),
            PersistentListenerMode.Void => UnityEventBase.GetValidMethodInfo(listenerType, name, new Type[0]),
            PersistentListenerMode.Float => UnityEventBase.GetValidMethodInfo(listenerType, name, new Type[1] { typeof(float) }),
            PersistentListenerMode.Int => UnityEventBase.GetValidMethodInfo(listenerType, name, new Type[1] { typeof(int) }),
            PersistentListenerMode.Bool => UnityEventBase.GetValidMethodInfo(listenerType, name, new Type[1] { typeof(bool) }),
            PersistentListenerMode.String => UnityEventBase.GetValidMethodInfo(listenerType, name, new Type[1] { typeof(string) }),
            PersistentListenerMode.Object => UnityEventBase.GetValidMethodInfo(listenerType, name, new Type[1] { argumentType ?? typeof(UnityEngine.Object) }),
            _ => null,
        };
    }

    internal static GenericMenu BuildPopupList(UnityEngine.Object target, UnityEventBase dummyEvent, SerializedProperty listener)
    {
        UnityEngine.Object @object = target;
        if (@object is Component)
        {
            @object = (target as Component).gameObject;
        }

        SerializedProperty serializedProperty = listener.FindPropertyRelative("m_MethodName");
        GenericMenu genericMenu = new GenericMenu();
        genericMenu.AddItem(new GUIContent("No Function"), string.IsNullOrEmpty(serializedProperty.stringValue), ClearEventFunction, new UnityEventFunction(listener, new ValidMethodMap(null, null, new PersistentListenerMode[1] { PersistentListenerMode.EventDefined}))); //??
        if (@object == null)
        {
            return genericMenu;
        }

        genericMenu.AddSeparator("");
        Type type = dummyEvent.GetType();
        MethodInfo method = type.GetMethod("Invoke");
        Type[] delegateArgumentsTypes = (from x in method.GetParameters()
                                         select x.ParameterType).ToArray();
        Dictionary<string, int> dictionary = CollectionPool<Dictionary<string, int>, KeyValuePair<string, int>>.Get();
        Dictionary<string, int> dictionary2 = CollectionPool<Dictionary<string, int>, KeyValuePair<string, int>>.Get();
        GeneratePopUpForType(genericMenu, @object, @object.GetType().Name, listener, delegateArgumentsTypes);
        dictionary[@object.GetType().Name] = 0;
        if (@object is GameObject)
        {
            Component[] components = (@object as GameObject).GetComponents<Component>();
            Component[] array = components;
            foreach (Component component in array)
            {
                if (!(component == null))
                {
                    int value = 0;
                    if (dictionary.TryGetValue(component.GetType().Name, out value))
                    {
                        value++;
                    }

                    dictionary[component.GetType().Name] = value;
                }
            }

            Component[] array2 = components;
            foreach (Component component2 in array2)
            {
                if (!(component2 == null))
                {
                    Type type2 = component2.GetType();
                    string targetName = type2.Name;
                    int value2 = 0;
                    if (dictionary[type2.Name] > 0)
                    {
                        targetName = ((!dictionary2.TryGetValue(type2.FullName, out value2)) ? type2.FullName : $"{type2.FullName} ({value2})");
                    }

                    GeneratePopUpForType(genericMenu, component2, targetName, listener, delegateArgumentsTypes);
                    dictionary2[type2.FullName] = value2 + 1;
                }
            }

            CollectionPool<Dictionary<string, int>, KeyValuePair<string, int>>.Release(dictionary);
            CollectionPool<Dictionary<string, int>, KeyValuePair<string, int>>.Release(dictionary2);
        }

        genericMenu.AddDisabledItem(new GUIContent("Delete Database"));

        return genericMenu;
    }

    private static void GeneratePopUpForType(GenericMenu menu, UnityEngine.Object target, string targetName, SerializedProperty listener, Type[] delegateArgumentsTypes)
    {
        List<ValidMethodMap> list = new List<ValidMethodMap>();
        bool flag = false;
        if (delegateArgumentsTypes.Length != 0)
        {
            GetMethodsForTargetAndMode(target, delegateArgumentsTypes, list);
            if (list.Count > 0)
            {
                menu.AddDisabledItem(new GUIContent(targetName + "/Dynamic " + string.Join(", ", delegateArgumentsTypes.Select((Type e) => GetTypeName(e)).ToArray())));
                AddMethodsToMenu(menu, listener, list, targetName);
                flag = true;
            }
        }

        list.Clear();

        //GetMethodsForTargetAndMode(target, new Type[1] { typeof(float) }, list, PersistentListenerMode.Float);
        //GetMethodsForTargetAndMode(target, new Type[1] { typeof(int) }, list, PersistentListenerMode.Int);
        //GetMethodsForTargetAndMode(target, new Type[1] { typeof(string) }, list, PersistentListenerMode.String);
        //GetMethodsForTargetAndMode(target, new Type[1] { typeof(bool) }, list, PersistentListenerMode.Bool);
        //GetMethodsForTargetAndMode(target, new Type[1] { typeof(UnityEngine.Object) }, list, PersistentListenerMode.Object);
        //GetMethodsForTargetAndMode(target, new Type[0], list, PersistentListenerMode.Void);

        GetMethodsForTargetAndMode(target, list);

        if (list.Count > 0)
        {
            if (flag)
            {
                menu.AddItem(new GUIContent(targetName + "/ "), on: false, null);
            }

            if (delegateArgumentsTypes.Length != 0)
            {
                menu.AddDisabledItem(new GUIContent(targetName + "/Static Parameters"));
            }

            AddMethodsToMenu(menu, listener, list, targetName);
        }
    }

    private static void AddMethodsToMenu(GenericMenu menu, SerializedProperty listener, List<ValidMethodMap> methods, string targetName)
    {
        IEnumerable<ValidMethodMap> enumerable = from e in methods
                                                 orderby (!e.m_methodInfo.Name.StartsWith("set_")) ? 1 : 0, e.m_methodInfo.Name
                                                 select e;
        foreach (ValidMethodMap item in enumerable)
        {
            AddFunctionsForScript(menu, listener, item, targetName);
        }
    }

    private static void GetMethodsForTargetAndMode(UnityEngine.Object target, Type[] delegateArgumentsTypes, List<ValidMethodMap> methods)
    {
        IEnumerable<ValidMethodMap> enumerable = CalculateMethodMap(target, delegateArgumentsTypes, false); //false because it is in dynamically alway set to the default value
        
        foreach (ValidMethodMap item in enumerable)
            methods.Add(item);
    }

    private static void GetMethodsForTargetAndMode(UnityEngine.Object target, List<ValidMethodMap> methods)
    {
        IEnumerable<ValidMethodMap> enumerable = CalculateMethodMap(target);
        
        foreach (ValidMethodMap item in enumerable)
            methods.Add(item);
    }

    //WRONG
    private static void AddFunctionsForScript(GenericMenu menu, SerializedProperty listener, ValidMethodMap method, string targetName)
    {
        PersistentListenerMode mode;

        if (method.m_modes.Length == 0)
            mode = PersistentListenerMode.Void;
        else
            mode = method.m_modes[0];

        UnityEngine.Object objectReferenceValue = listener.FindPropertyRelative("m_Target").objectReferenceValue;
        string stringValue = listener.FindPropertyRelative("m_MethodName").stringValue;
        PersistentListenerMode mode2 = GetMode(listener.FindPropertyRelative("m_Mode"));
        SerializedProperty serializedProperty = listener.FindPropertyRelative("m_Arguments").FindPropertyRelative("m_ObjectArgumentAssemblyTypeName");
        StringBuilder stringBuilder = new StringBuilder();
        int num = method.m_methodInfo.GetParameters().Length;
        for (int i = 0; i < num; i++)
        {
            ParameterInfo parameterInfo = method.m_methodInfo.GetParameters()[i];
            stringBuilder.Append($"{GetTypeName(parameterInfo.ParameterType)}");
            if (i < num - 1)
            {
                stringBuilder.Append(", ");
            }
        }

        bool flag = objectReferenceValue == method.m_target && stringValue == method.m_methodInfo.Name && mode == mode2;
        if (flag && mode == PersistentListenerMode.Object && method.m_methodInfo.GetParameters().Length == 1)
        {
            flag &= method.m_methodInfo.GetParameters()[0].ParameterType.AssemblyQualifiedName == serializedProperty.stringValue;
        }

        string formattedMethodName = GetFormattedMethodName(targetName, method.m_methodInfo.Name, stringBuilder.ToString(), mode == PersistentListenerMode.EventDefined);
        menu.AddItem(new GUIContent(formattedMethodName), flag, SetEventFunction, new UnityEventFunction(listener, new ValidMethodMap(method.m_target, method.m_methodInfo, method.m_modes))); //--> change this to just modes back
    }

    private static string GetTypeName(Type t)
    {
        if (t == typeof(int))
        {
            return "int";
        }

        if (t == typeof(float))
        {
            return "float";
        }

        if (t == typeof(string))
        {
            return "string";
        }

        if (t == typeof(bool))
        {
            return "bool";
        }

        return t.Name;
    }

    private static string GetFormattedMethodName(string targetName, string methodName, string args, bool dynamic)
    {
        if (dynamic)
        {
            if (methodName.StartsWith("set_"))
            {
                return $"{targetName}/{methodName.Substring(4)}";
            }

            return $"{targetName}/{methodName}";
        }

        if (methodName.StartsWith("set_"))
        {
            return string.Format("{0}/{2} {1}", targetName, methodName.Substring(4), args);
        }

        return $"{targetName}/{methodName} ({args})";
    }

    private static void SetEventFunction(object source)
    {
        ((UnityEventFunction)source).Assign();
    }

    private static void ClearEventFunction(object source)
    {
        ((UnityEventFunction)source).Clear();
    }
}

/**/