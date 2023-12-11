using ScriptableArchitecture.Core;
using UnityEngine;
using UnityEngine.Events;

public class Tes : MonoBehaviour
{
    public UnityEvent _event;

    [ContextMenu("AddEvent")]
    public void AddEvent()
    {
        //UnityEventTools.AddObjectPersistentListener(_event, );
    }

    public void RaiseEvent(EventData dataPoint)
    {

    }

    public void T0(int T1)
    {
        Debug.Log(T1);
    }

    public void Test1(int T1, int T2)
    {
        Debug.Log(T1 + " " + T2);
    }

    public void Test2(int T1, string T2)
    {
        Debug.Log(T1 + " " + T2);
    }

    public void Test3(int T1, string T2, float T3)
    {
       Debug.Log(T1 + " " + T2 + " " + T3);
    }

    public int Test4()
    {
        return 0;
    }

    public int Test5(int T1)
    {
        Debug.Log(T1);
        return 0;
    }

    public int Test6(int T1, int T2)
    {
        Debug.Log(T1 + " " + T2);
        return 0;
    }
}