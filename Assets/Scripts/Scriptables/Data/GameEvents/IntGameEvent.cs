using ScriptableArchitecture.Core;
using UnityEngine;

namespace ScriptableArchitecture.Data
{
    [CreateAssetMenu(fileName = "IntGameEvent", menuName = "Scriptables/GameEvents/IntGameEvent")]
    public class IntGameEvent : GameEventBase<int>
    {
    }
}