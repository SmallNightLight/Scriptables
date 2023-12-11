using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace ScriptableArchitecture.Data
{
    public class AudioManager : MonoBehaviour
    {
        public static SoundMode SOUNDMODE;
        public AudioMixer MainMixer;

        public SoundMode SoundMode = SOUNDMODE;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (SOUNDMODE != SoundMode)
                SOUNDMODE = SoundMode;
        }
#endif

        private List<AudioSource> _audioSources = new List<AudioSource>();

        public void Start()
        {
            if (SoundMode == SoundMode.MixerBased)
            {
                foreach (AudioMixerGroup mixerGroup in AllMixerGroups)
                {
                    GameObject g = new GameObject();
                    g.name = mixerGroup.name;
                    g.transform.parent = transform;

                    AudioSource source = g.AddComponent<AudioSource>();
                    source.outputAudioMixerGroup = mixerGroup;
                    source.playOnAwake = false;

                    _audioSources.Add(source);
                }
            }
        }

        public AudioMixerGroup[] AllMixerGroups
        {
            get
            {
                return MainMixer.FindMatchingGroups(string.Empty);
            }
        }

        public void PlaySoundEffect(SoundEffect soundEffect)
        {
            if (SoundMode == SoundMode.CategoryBased) //For WebGL
            {

            }
            else if (SoundMode == SoundMode.MixerBased)
            {
                AudioMixerGroup audioMixerGroup = soundEffect.audioMixerSnapshot;
                AudioSource audioSource = _audioSources.FirstOrDefault(source => source.outputAudioMixerGroup == audioMixerGroup);
                if (audioMixerGroup.name != "Music" && !soundEffect.Loop)
                    audioSource.PlayOneShot(soundEffect.AudioClip, soundEffect.Volume);
                else
                {
                    audioSource.clip = soundEffect.AudioClip;
                    audioSource.loop = true;
                    audioSource.volume = soundEffect.Volume;
                    audioSource.Play();
                }

                //to set sound on mixer: Mathf.Log10(soundEffect.Volume * soundCategory.Volume) * 20
            }
        }

        public void FadeOutMixer(AudioMixerGroup audioMixerGroup)
        {
            _audioSources.FirstOrDefault(audioSource => audioSource.outputAudioMixerGroup == audioMixerGroup).Stop();
        }

    }

    public enum SoundMode
    {
        MixerBased, CategoryBased
    }
}