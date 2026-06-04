using UnityEngine;

[System.Serializable]
public class SoundEffectData
{
    public string soundName;
    public AudioClip[] clips;
    public SfxPlayMode playMode = SfxPlayMode.GameplayOnly;
    [Range(0f, 2f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitchMin = 1f;
    [Range(0.1f, 3f)] public float pitchMax = 1f;
}