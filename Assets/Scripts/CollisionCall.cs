using UnityEngine;
using UnityEngine.Events;

public class CollisionCall : MonoBehaviour
{
    [SerializeField] private CollisionType collisionType;
    [SerializeField] private ListenType listenType;

    //[ShowIf("listenType", ListenType.Tag)]
    //[Tag]
    [SerializeField] private string listenTag;

    //[ShowIf("listenType", ListenType.Script)]
    [SerializeField] MonoBehaviour listenScript;


    //[ShowIf("collisionType", CollisionType2D.TriggerEnter)]
    [SerializeField] private UnityEvent EventTriggerEnter;

    //[ShowIf("collisionType", CollisionType2D.TriggerExit)]
    [SerializeField] private UnityEvent EventTriggerExit;

    //[ShowIf("collisionType", CollisionType2D.TriggerStay)]
    [SerializeField] private UnityEvent EventTriggerStay;

    //[ShowIf("collisionType", CollisionType2D.CollisionEnter)]
    [SerializeField] private UnityEvent EventCollisionEnter;

    //[ShowIf("collisionType", CollisionType2D.CollisionExit)]
    [SerializeField] private UnityEvent EventCollisionExit;

    //[ShowIf("collisionType", CollisionType2D.CollisionStay)]
    [SerializeField] private UnityEvent EventCollisionStay;


    private enum ListenType
    {
        None,
        Tag,
        Script
    }

    private enum CollisionType
    {
        None,
        TriggerEnter,
        TriggerExit,
        TriggerStay,
        CollisionEnter,
        CollisionExit,
        CollisionStay
    }

    private void InvokeEvent(UnityEvent unityEvent, GameObject other)
    {
        if (listenType == ListenType.Tag && other.CompareTag(listenTag))
            unityEvent.Invoke();
        else if (listenType == ListenType.Script && other.TryGetComponent(out MonoBehaviour script))
            if (script == listenScript)
                unityEvent.Invoke();
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collisionType == CollisionType.TriggerEnter)
            InvokeEvent(EventTriggerEnter, collision.gameObject);
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collisionType == CollisionType.TriggerExit)
            InvokeEvent(EventTriggerExit, collision.gameObject);
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collisionType == CollisionType.TriggerStay)
            InvokeEvent(EventTriggerStay, collision.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collisionType == CollisionType.CollisionEnter)
            InvokeEvent(EventCollisionEnter, collision.gameObject);
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collisionType == CollisionType.TriggerExit)
            InvokeEvent(EventCollisionExit, collision.gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collisionType == CollisionType.CollisionStay)
            InvokeEvent(EventCollisionStay, collision.gameObject);
    }
}
