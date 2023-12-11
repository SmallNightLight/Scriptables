using ScriptableArchitecture.Core;
using UnityEngine;

namespace ScriptableArchitecture.Data
{
    [CreateAssetMenu(fileName = "TransformVariable", menuName = "Scriptables/Variables/Transform")]
    public class TransformVariable : Variable<Transform>
    {
    }
}