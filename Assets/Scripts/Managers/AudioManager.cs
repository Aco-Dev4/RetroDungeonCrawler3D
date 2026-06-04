using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SfxPlayMode
{
    GameplayOnly,
    UI,
    Always
}

[System.Serializable]
public class SceneMusicEntry
{
    public string sceneName;
    public AudioClip musicClip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Scene Music")]
    [SerializeField] private List<SceneMusicEntry> sceneMusic = new();

    [Header("SFX Database")]
    [SerializeField] private SoundEffectDatabase soundEffectDatabase;

    private AudioClip _currentMusic;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        RefreshVolumes();
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshVolumes();
        PlayMusicForScene(scene.name);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource == null) return;
        if (clip == null) return;
        if (_currentMusic == clip && musicSource.isPlaying)
            return;

        _currentMusic = clip;

        musicSource.clip = clip;
        musicSource.loop = true;

        RefreshVolumes();

        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource == null) return;

        musicSource.Stop();
        _currentMusic = null;
    }

    public void PlayMusicForScene(string sceneName)
    {
        for (int i = 0; i < sceneMusic.Count; i++)
        {
            if (sceneMusic[i] == null) continue;
            if (sceneMusic[i].sceneName != sceneName) continue;

            PlayMusic(sceneMusic[i].musicClip);
            return;
        }
    }

    public void PlaySFX(string soundName)
    {
        if (soundEffectDatabase == null) return;

        SoundEffectData sound = soundEffectDatabase.GetSound(soundName);

        if (sound == null)
        {
            Debug.LogWarning($"Missing SFX: {soundName}");
            return;
        }

        if (sound.clips == null || sound.clips.Length == 0) return;
        if (!CanPlaySFX(sound.playMode)) return;

        AudioClip clip = sound.clips[Random.Range(0, sound.clips.Length)];
        float pitch = Random.Range(sound.pitchMin, sound.pitchMax);

        sfxSource.pitch = pitch;
        sfxSource.PlayOneShot(clip, sound.volume);
        sfxSource.pitch = 1f;
    }

    private bool CanPlaySFX(SfxPlayMode playMode)
    {
        if (playMode == SfxPlayMode.Always)
            return true;

        if (playMode == SfxPlayMode.UI)
            return true;

        if (GameManager.Instance == null)
            return true;

        return GameManager.Instance.State == GameState.Playing;
    }

    public void RefreshVolumes()
    {
        if (GameDataManager.Instance == null) return;

        AudioListener.volume = GameDataManager.Instance.GetMasterVolume();

        if (musicSource != null)
            musicSource.volume = GameDataManager.Instance.GetMusicVolume();

        if (sfxSource != null)
            sfxSource.volume = GameDataManager.Instance.GetSfxVolume();
    }

    private float GetSfxVolume()
    {
        return GameDataManager.Instance != null ? GameDataManager.Instance.GetSfxVolume() : 1f;
    }
}