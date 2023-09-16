using ScriptableArchitecture.Core;
using UnityEngine;

namespace ScriptableArchitecture.Data
{
    [CreateAssetMenu(fileName = "FloatGameEvent", menuName = "Scriptables/GameEvents/FloatGameEvent")]
    public class FloatGameEvent : GameEventBase<float>
    {
    }
}