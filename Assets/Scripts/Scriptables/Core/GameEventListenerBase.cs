using UnityEngine;
using UnityEngine.Events;

namespace ScriptableArchitecture.Core
{
    public abstract class GameEventListenerBase : MonoBehaviour
    {
        [SerializeField] private GameEventBase _event;
        [SerializeField] private UnityEvent _response;

        public void OnEventRaised()
        {
            _response.Invoke();
        }

        private void OnEnable()
        {
            _event.RegisterListener(this);
        }

        private void OnDisable()
        {
            _event.UnregisterListener(this);
        }
    }

    public abstract class GameEventListenerBase<T> : MonoBehaviour
    {
        [SerializeField] private GameEventBase<T> _event;
        [SerializeField] private UnityEvent<T> _response;

        public void OnEventRaised(T value)
        {
            _response.Invoke(value);
        }

        private void OnEnable()
        {
            if (_event == null)
            {
                Debug.LogWarning("No Event selected");
                return;
            }

            _event.RegisterListener(this);
        }

        private void OnDisable()
        {
            if (_event == null)
                return;

            _event.UnregisterListener(this);
        }
    }
}