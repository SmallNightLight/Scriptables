using System.Collections.Generic;
using UnityEngine;

namespace ScriptableArchitecture.Core
{
    public class GameEventBase : ScriptableObject
    {
        private List<GameEventListenerBase> _listeners = new List<GameEventListenerBase>();

        public void Raise()
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i].OnEventRaised();
        }
        
        public void RegisterListener(GameEventListenerBase listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void UnregisterListener(GameEventListenerBase listener)
        {
            if (_listeners.Contains(listener))
                _listeners.Remove(listener);
        }

        public void RemoveAllListeners()
        {
            _listeners.Clear();
        }
    }

    public class GameEventBase<T> : ScriptableObject
    {
        private List<GameEventListenerBase<T>> _listeners = new List<GameEventListenerBase<T>>();

        public T DebugValue;

        public void Raise(T value)
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
                _listeners[i].OnEventRaised(value);
        }

        public void RegisterListener(GameEventListenerBase<T> listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public void UnregisterListener(GameEventListenerBase<T> listener)
        {
            if (_listeners.Contains(listener))
                _listeners.Remove(listener);
        }

        public void RemoveAllListeners()
        {
            _listeners.Clear();
        }
    }
}