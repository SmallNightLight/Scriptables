using ScriptableArchitecture.Core;
using UnityEngine;

namespace ScriptableArchitecture.Data
{
    [CreateAssetMenu(fileName = "TransformGameEvent", menuName = "Scriptables/GameEvents/TransformGameEvent")]
    public class TransformGameEvent : GameEventBase<Transform>
    {
    }
}