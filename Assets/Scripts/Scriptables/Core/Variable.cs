using UnityEngine;

namespace ScriptableArchitecture.Core
{
    public abstract class Variable<T> : Variable
    {
        [SerializeField]
        public T Value;
        public VariableType Type;
        public T DefaultValue;

        //OnEnable is called when the first scene is loaded, regardles if the scene has a reference to it (build game)
        private void OnEnable()
        {
            //This is only for the editor as the build game does not save Scriptable objects across sessions
            if (Type == VariableType.ResetOnGameStart)
                Value = DefaultValue;
        }

        public void Set(T data)
        {
            Value = data;
        }
    }

    public abstract class Variable : ScriptableObject
    {
    }

    public enum VariableType
    {
        Normal, ResetOnGameStart //, Persistent
    }
}