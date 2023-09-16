using ScriptableArchitecture.Core;
using UnityEngine;

namespace ScriptableArchitecture.Data
{
    [CreateAssetMenu(fileName = "BoolGameEvent", menuName = "Scriptables/GameEvents/BoolGameEvent")]
    public class BoolGameEvent : GameEventBase<bool>
    {
    }
}