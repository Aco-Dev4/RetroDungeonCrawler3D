using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    #region References
    [Header("Panels")]
    [SerializeField] private GameObject mapSelectPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private SettingsManager settingsManager;

    [Header("Map Buttons")]
    [SerializeField] private List<MenuMapButton> mapButtons = new();

    [Header("Difficulty Panel")]
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private Button normalDifficultyButton;
    [SerializeField] private Button hardDifficultyButton;
    [SerializeField] private GameObject hardLockedVisual;
    [SerializeField] private string selectedMapSceneName;
    [SerializeField] private string selectedMapId;

    [Header("Shop")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private MenuShopCamera shopCamera;
    #endregion

    private void Start()
    {
        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        RefreshMapButtons();
    }

    #region Buttons
    public void OnStartPressed()
    {
        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(true);

        RefreshMapButtons();
    }

    public void OnCloseMapSelectPressed()
    {
        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(false);
    }

    public void RefreshMapButtons()
    {
        for (int i = 0; i < mapButtons.Count; i++)
        {
            if (mapButtons[i] != null)
                mapButtons[i].RefreshState();
        }
    }

    public void OpenDifficultyPanel(string sceneName, string mapId)
    {
        selectedMapSceneName = sceneName;
        selectedMapId = mapId;

        if (difficultyPanel != null)
            difficultyPanel.SetActive(true);

        bool hardUnlocked = GameDataManager.Instance != null && GameDataManager.Instance.HasCompletedMap(mapId);

        if (normalDifficultyButton != null)
            normalDifficultyButton.interactable = true;

        if (hardDifficultyButton != null)
            hardDifficultyButton.interactable = hardUnlocked;

        if (hardLockedVisual != null)
            hardLockedVisual.SetActive(!hardUnlocked);
    }

    public void OnNormalDifficultyPressed()
    {
        LoadSelectedMap(RunDifficulty.Normal);
    }

    public void OnHardDifficultyPressed()
    {
        LoadSelectedMap(RunDifficulty.Hard);
    }

    public void OnCloseDifficultyPanelPressed()
    {
        if (difficultyPanel != null)
            difficultyPanel.SetActive(false);
    }

    private void LoadSelectedMap(RunDifficulty difficulty)
    {
        if (string.IsNullOrWhiteSpace(selectedMapSceneName)) return;

        SelectedRunSettings.Difficulty = difficulty;
        SceneManager.LoadScene(selectedMapSceneName);
    }

    public void OnShopPressed()
    {
        if (shopPanel != null)
            shopPanel.SetActive(true);

        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(false);

        shopCamera?.MoveToShopView();
    }

    public void OnCloseShopPressed()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        shopCamera?.MoveToNormalView();
    }

    public void OnSwordBought()
    {
        GameDataManager.Instance?.UnlockSword();
        //Debug.Log("Sword bought");
    }

    public void OnSettingsPressed()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        settingsManager?.LoadSettingsToUI();

        if (mapSelectPanel != null)
            mapSelectPanel.SetActive(false);

        if (shopPanel != null)
            shopPanel.SetActive(false);

        shopCamera?.MoveToNormalView();
    }

    public void OnCloseSettingsPressed()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void OnExitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
    #endregion
}
