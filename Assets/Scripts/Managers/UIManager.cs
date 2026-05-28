using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    [SerializeField] private GameObject pausePanel;

    [Header("Panels closed on Game Over")]
    [SerializeField] private GameObject[] closeOnGameOverPanels;

    [Header("Tutorial Game Over")]
    [SerializeField] private bool useTutorialGameOver;
    [SerializeField] private GameObject tutorialGameOverPanel;
    [SerializeField] private CanvasGroup tutorialGameOverCanvasGroup;

    [Header("Game Over Stats")]
    [SerializeField] private TMP_Text goldGainedText;
    [SerializeField] private TMP_Text silverGainedText;
    [SerializeField] private TMP_Text enemiesDefeatedText;
    [SerializeField] private TMP_Text wavesCompletedText;
    [SerializeField] private TMP_Text diedOnWaveText;

    [Header("Game Over Settings")]
    [SerializeField] private float gameOverDelay = 1.5f;
    [SerializeField] private float fadeDuration = 1f;

    private bool _isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.alpha = 0f;
            gameOverCanvasGroup.interactable = false;
            gameOverCanvasGroup.blocksRaycasts = false;
        }

        if (tutorialGameOverCanvasGroup != null)
        {
            tutorialGameOverCanvasGroup.alpha = 0f;
            tutorialGameOverCanvasGroup.interactable = false;
            tutorialGameOverCanvasGroup.blocksRaycasts = false;
        }

        if (tutorialGameOverPanel != null)
            tutorialGameOverPanel.SetActive(false);
    }

    #region Game Over

    public void ShowGameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        // Close other panels
        foreach (var panel in closeOnGameOverPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        if (pausePanel != null)
            pausePanel.SetActive(false);

        CursorManager.Instance?.UnlockCursor();

        yield return new WaitForSeconds(gameOverDelay);

        if (useTutorialGameOver)
        {
            if (tutorialGameOverPanel != null)
                tutorialGameOverPanel.SetActive(true);

            if (tutorialGameOverCanvasGroup != null)
                yield return FadeInCanvasGroup(tutorialGameOverCanvasGroup);
        }
        else
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            UpdateGameOverStats();

            if (gameOverCanvasGroup != null)
                yield return FadeInCanvasGroup(gameOverCanvasGroup);
        }
    }

    private IEnumerator FadeInCanvasGroup(CanvasGroup canvasGroup)
    {
        float t = 0f;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private void UpdateGameOverStats()
    {
        if (RunStatsManager.Instance == null) return;

        if (goldGainedText != null)
            goldGainedText.text = $"GOLD GAINED: {RunStatsManager.Instance.GoldGained}";

        if (silverGainedText != null)
            silverGainedText.text = $"SILVER GAINED: {RunStatsManager.Instance.SilverGained}";

        if (enemiesDefeatedText != null)
            enemiesDefeatedText.text = $"ENEMIES DEFEATED: {RunStatsManager.Instance.EnemiesDefeated}";

        if (wavesCompletedText != null)
            wavesCompletedText.text = $"WAVES COMPLETED: {RunStatsManager.Instance.WavesCompleted}";

        if (diedOnWaveText != null)
            diedOnWaveText.text = $"DIED ON WAVE: {RunStatsManager.Instance.CurrentWave}";
    }
    #endregion

    #region Pause

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        if (ChestRewardUI.Instance != null && ChestRewardUI.Instance.IsOpen())
            return;

        if (CardUpgradeUI.Instance != null && CardUpgradeUI.Instance.IsOpen())
        {
            CardUpgradeUI.Instance.Close();
            return;
        }

        TogglePause();
    }

    public void TogglePause()
    {
        if (_isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        if (pausePanel == null) return;
        _isPaused = true;
        pausePanel.SetActive(true);

        GameManager.Instance.PauseGame();
    }

    public void ResumeGame()
    {
        if (pausePanel == null) return;
        _isPaused = false;
        pausePanel.SetActive(false);

        GameManager.Instance.ResumeGame();
    }

    #endregion

    #region Buttons

    public void OnSettingsPressed()
    {
        Debug.Log("Settings panel will show up later");
    }

    public void OnMenuPressed()
    {
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void OnRestartScenePressed()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    #endregion
}



