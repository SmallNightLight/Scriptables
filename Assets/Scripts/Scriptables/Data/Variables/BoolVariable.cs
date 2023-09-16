using ScriptableArchitecture.Core;
using UnityEngine;

namespace ScriptableArchitecture.Data
{
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "Scriptables/Variables/Bool")]
    public class BoolVariable : Variable<bool>
    {
    }
}