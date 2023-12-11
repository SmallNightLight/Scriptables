using ScriptableArchitecture.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace ScriptableArchitecture.Data
{
    [System.Serializable]
    public struct SoundEffect : IDataPoint
    {
        public AudioMixerGroup audioMixerSnapshot;

        public AudioClip AudioClip;
        [Range(0, 5)]
        public float Volume;
        [Range(0, 3)]
        public float Pitch;

        public bool Loop;
    }
}