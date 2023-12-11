using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResponseEvent
{
    //A response Event
    [SerializeField] private List<Action> _actions = new List<Action>();

    public void Invoke()
    {
        for(int i = 0; i < _actions.Count; i++)
            _actions[i].Invoke();
    }

    public void AddResponse(Action action)
    {
        if (!_actions.Contains(action))
            _actions.Add(action);
    }

    public void RemoveResponse(Action action)
    {
        if (_actions.Contains(action))
            _actions.Remove(action);
    }

    public void ActionTest(int i1, int i2)
    {

    }
}