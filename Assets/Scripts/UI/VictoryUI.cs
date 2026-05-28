using TMPro;
using UnityEngine;
using System.Collections;

public class VictoryUI : MonoBehaviour
{
    public static VictoryUI Instance;

    [Header("Panel")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private CanvasGroup victoryCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Texts")]
    [SerializeField] private TMP_Text mapCompletedText;
    [SerializeField] private TMP_Text goldGainedText;
    [SerializeField] private TMP_Text silverGainedText;
    [SerializeField] private TMP_Text enemiesDefeatedText;
    [SerializeField] private TMP_Text cardsUpgradedText;
    [SerializeField] private TMP_Text timeText;

    private void Awake()
    {
        Instance = this;

        if (victoryCanvasGroup != null)
        {
            victoryCanvasGroup.alpha = 0f;
            victoryCanvasGroup.interactable = false;
            victoryCanvasGroup.blocksRaycasts = false;
        }

        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public void Show(string mapName)
    {
        if (rootPanel != null)
            rootPanel.SetActive(true);

        GameManager.Instance?.GameOver();

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            CameraManager.Instance?.StartOrbit(player.GetOrbitPivot());
            player.enabled = false;
        }

        if (mapCompletedText != null)
            mapCompletedText.text = $"{mapName.ToUpper()} MAP COMPLETED!";

        if (RunStatsManager.Instance != null)
        {
            if (goldGainedText != null)
                goldGainedText.text = $"GOLD GAINED: {RunStatsManager.Instance.GoldGained}";

            if (silverGainedText != null)
                silverGainedText.text = $"SILVER GAINED: {RunStatsManager.Instance.SilverGained}";

            if (enemiesDefeatedText != null)
                enemiesDefeatedText.text = $"ENEMIES DEFEATED: {RunStatsManager.Instance.EnemiesDefeated}";

            if (cardsUpgradedText != null)
                cardsUpgradedText.text = $"CARDS UPGRADED: {RunStatsManager.Instance.CardsUpgraded}";
        }

        if (timeText != null)
            timeText.text = $"TIME: {(RunTimer.Instance != null ? RunTimer.Instance.GetFormattedTime() : "0.00")}";

        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SetMapCompleted(mapName);

        StartCoroutine(FadeInVictory());
    }

    private IEnumerator FadeInVictory()
    {
        if (victoryCanvasGroup == null)
            yield break;

        float t = 0f;

        victoryCanvasGroup.alpha = 0f;
        victoryCanvasGroup.interactable = true;
        victoryCanvasGroup.blocksRaycasts = true;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            victoryCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        victoryCanvasGroup.alpha = 1f;
    }
}