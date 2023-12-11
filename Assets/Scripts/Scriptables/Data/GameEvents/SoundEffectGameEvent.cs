using ScriptableArchitecture.Core;
using UnityEngine;

namespace ScriptableArchitecture.Data
{
    [CreateAssetMenu(fileName = "SoundEffectGameEvent", menuName = "Scriptables/GameEvents/SoundEffectGameEvent")]
    public class SoundEffectGameEvent : GameEventBase<SoundEffect>
    {
        public void Raise(SoundEffectVariable sound)
        {
            Raise(sound.Value);
        }
    }
}