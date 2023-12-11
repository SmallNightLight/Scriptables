using UnityEditor;
using UnityEngine;

namespace ScriptableArchitecture.Data
{
    public class SoundToSO
    {
        [MenuItem("Convert/Sound/Sound to SO - Sound Effect")]
        static void ConvertSoundToSOSoundEffect()
        {
            string[] guids;

            guids = AssetDatabase.FindAssets("t:AudioClip");
            foreach (string guid in guids)
            {
                string audioClipPath = AssetDatabase.GUIDToAssetPath(guid);
                string audioClipFolder = "/SoundEffects/";

                if (audioClipPath.Contains(audioClipFolder))
                {
                    SoundEffectVariable asset = ScriptableObject.CreateInstance<SoundEffectVariable>();
                    int index = audioClipPath.IndexOf(audioClipFolder) + audioClipFolder.Length;
                    string s = audioClipPath.Substring(index);
                    string path = "Assets/Data/Sound/SoundEffects/" + s;
                    string[] folders = path.Split("/");
                    string previousPath = folders[0];

                    for (int i = 1; i < folders.Length - 1; i++)
                    {
                        previousPath += "/" + folders[i];
                        if (!AssetDatabase.IsValidFolder(previousPath))
                            AssetDatabase.CreateFolder(previousPath.Substring(0, previousPath.LastIndexOf("/")), previousPath.Substring(previousPath.LastIndexOf("/") + 1));
                    }

                    asset.Value.AudioClip = (AudioClip)AssetDatabase.LoadAssetAtPath(audioClipPath, typeof(AudioClip));

                    string newPath = path.Substring(0, path.LastIndexOf(".")) + ".asset";

                    if (AssetDatabase.GetMainAssetTypeAtPath(newPath) == null)
                    {
                        AssetDatabase.CreateAsset(asset, newPath);
                        Debug.Log("Created new SoundEffect: " + asset.name + " at: " + newPath);
                    }
                }
                else
                {
                    Debug.Log("Couldn't convert: " + audioClipPath);
                }
            }
        }
    }
}