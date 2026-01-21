using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CanvasGroup gameOverCanvasGroup;
    [SerializeField] private GameObject pausePanel;

    [Header("Panels closed on Game Over")]
    [SerializeField] private GameObject[] closeOnGameOverPanels;

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

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverCanvasGroup != null)
            yield return FadeInGameOver();
    }

    private IEnumerator FadeInGameOver()
    {
        float t = 0f;

        gameOverCanvasGroup.interactable = true;
        gameOverCanvasGroup.blocksRaycasts = true;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            gameOverCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        gameOverCanvasGroup.alpha = 1f;
    }

    #endregion

    #region Pause

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.started) return;
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
        Debug.Log("Menu scene will be added later");
    }

    #endregion
}



