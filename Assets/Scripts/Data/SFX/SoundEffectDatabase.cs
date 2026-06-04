using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Audio/Sound Effect Database")]
public class SoundEffectDatabase : ScriptableObject
{
    public List<SoundEffectData> sounds = new();

    public SoundEffectData GetSound(string soundName)
    {
        for (int i = 0; i < sounds.Count; i++)
        {
            if (sounds[i] != null && sounds[i].soundName == soundName)
                return sounds[i];
        }

        return null;
    }
}