using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class SettingsManager : MonoBehaviour
{
    #region References
    [Header("Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sensitivitySlider;

    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown graphicsDropdown;

    [Header("Camera")]
    [SerializeField] private CinemachineInputAxisController cameraInputController;
    #endregion

    #region Runtime
    private readonly List<Resolution> _resolutions = new();
    #endregion

    #region Unity
    private void Awake()
    {
        SetupResolutionDropdown();
        SetupGraphicsDropdown();
    }

    private void OnEnable()
    {
        LoadSettingsToUI();
    }
    #endregion

    #region Setup
    private void SetupResolutionDropdown()
    {
        if (resolutionDropdown == null) return;

        resolutionDropdown.ClearOptions();
        _resolutions.Clear();

        List<string> options = new();
        Resolution[] availableResolutions = Screen.resolutions;

        for (int i = 0; i < availableResolutions.Length; i++)
        {
            Resolution resolution = availableResolutions[i];

            if (ContainsResolution(resolution.width, resolution.height))
                continue;

            _resolutions.Add(resolution);
            options.Add($"{resolution.width}x{resolution.height}");
        }

        resolutionDropdown.AddOptions(options);
    }

    private bool ContainsResolution(int width, int height)
    {
        for (int i = 0; i < _resolutions.Count; i++)
        {
            if (_resolutions[i].width == width && _resolutions[i].height == height)
                return true;
        }

        return false;
    }

    private void SetupGraphicsDropdown()
    {
        if (graphicsDropdown == null) return;

        graphicsDropdown.ClearOptions();
        graphicsDropdown.AddOptions(new List<string> { "LOW", "MEDIUM", "HIGH" });
    }
    #endregion

    #region Load / Apply
    public void LoadSettingsToUI()
    {
        if (GameDataManager.Instance == null) return;

        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(GameDataManager.Instance.GetMasterVolume());

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(GameDataManager.Instance.GetSfxVolume());

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(GameDataManager.Instance.GetMusicVolume());

        if (sensitivitySlider != null)
        {
            sensitivitySlider.SetValueWithoutNotify(GameDataManager.Instance.GetSensitivity());
            ApplySensitivity(GameDataManager.Instance.GetSensitivity());
        }

        if (graphicsDropdown != null)
        {
            graphicsDropdown.SetValueWithoutNotify(GameDataManager.Instance.GetQualityLevel());
            graphicsDropdown.RefreshShownValue();
        }

        if (resolutionDropdown != null)
        {
            int resolutionIndex = GameDataManager.Instance.GetResolutionIndex();

            if (resolutionIndex < 0)
                resolutionIndex = GetCurrentResolutionIndex();

            resolutionIndex = Mathf.Clamp(resolutionIndex, 0, Mathf.Max(0, _resolutions.Count - 1));

            resolutionDropdown.SetValueWithoutNotify(resolutionIndex);
            resolutionDropdown.RefreshShownValue();
        }

        Canvas.ForceUpdateCanvases();

        //Debug.Log("Settings UI loaded.");
    }

    public void ApplySettings()
    {
        if (GameDataManager.Instance == null) return;

        if (masterVolumeSlider != null)
            GameDataManager.Instance.SetMasterVolume(masterVolumeSlider.value);

        if (sfxVolumeSlider != null)
            GameDataManager.Instance.SetSfxVolume(sfxVolumeSlider.value);

        if (musicVolumeSlider != null)
            GameDataManager.Instance.SetMusicVolume(musicVolumeSlider.value);

        if (sensitivitySlider != null)
        {
            GameDataManager.Instance.SetSensitivity(sensitivitySlider.value);
            ApplySensitivity(sensitivitySlider.value);
        }

        if (graphicsDropdown != null)
        {
            int quality = Mathf.Clamp(graphicsDropdown.value, 0, 2);
            QualitySettings.SetQualityLevel(quality);
            ApplyExtraGraphicsSettings(quality);
            GameDataManager.Instance.SetQualityLevel(quality);
        }

        if (resolutionDropdown != null)
        {
            int resolutionIndex = resolutionDropdown.value;
            GameDataManager.Instance.SetResolutionIndex(resolutionIndex);

#if !UNITY_EDITOR
            if (resolutionIndex >= 0 && resolutionIndex < _resolutions.Count)
            {
                Resolution resolution = _resolutions[resolutionIndex];
                Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
            }
#endif
        }

        //Debug.Log("Settings applied and saved.");
    }

    private void ApplySensitivity(float sensitivity)
    {
        if (cameraInputController == null)
            return;

        cameraInputController.Controllers[0].Input.Gain = sensitivity;
        cameraInputController.Controllers[1].Input.Gain = -sensitivity;
    }
    #endregion

    #region Helpers
    private int GetCurrentResolutionIndex()
    {
        for (int i = 0; i < _resolutions.Count; i++)
        {
            if (_resolutions[i].width == Screen.currentResolution.width &&
                _resolutions[i].height == Screen.currentResolution.height)
                return i;
        }

        return Mathf.Max(0, _resolutions.Count - 1);
    }

    private void ApplyExtraGraphicsSettings(int qualityIndex)
    {
        switch (qualityIndex)
        {
            case 0:
                QualitySettings.shadowDistance = 25f;
                QualitySettings.antiAliasing = 0;
                break;

            case 1:
                QualitySettings.shadowDistance = 60f;
                QualitySettings.antiAliasing = 2;
                break;

            case 2:
                QualitySettings.shadowDistance = 120f;
                QualitySettings.antiAliasing = 4;
                break;
        }
    }
    #endregion
}