using ScriptableArchitecture.Core;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

namespace ScriptableArchitecture.Data
{
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "Scriptables/Variables/Bool")]
    public class BoolVariable : Variable<bool>
    {
        public void Invert()
        {
            Value = !Value;
        }
    }
}