using TMPro;
using UnityEngine;

public class NextWavePromptUI : MonoBehaviour
{
    [Header("Big Prompt")]
    [SerializeField] private GameObject bigPromptRoot;
    [SerializeField] private TMP_Text bigPromptText;

    [Header("Small Prompt")]
    [SerializeField] private GameObject smallPromptRoot;
    [SerializeField] private TMP_Text smallPromptText;

    public void SetPrompt(int nextWaveNumber, bool hasActiveWaves, bool canStartMoreWaves)
    {
        if (!canStartMoreWaves)
        {
            HideAll();
            return;
        }

        if (hasActiveWaves)
            ShowSmall(nextWaveNumber);
        else
            ShowBig(nextWaveNumber);
    }

    public void HideAll()
    {
        if (bigPromptRoot != null)
            bigPromptRoot.SetActive(false);

        if (smallPromptRoot != null)
            smallPromptRoot.SetActive(false);
    }

    private void ShowBig(int nextWaveNumber)
    {
        if (bigPromptRoot != null)
            bigPromptRoot.SetActive(true);

        if (smallPromptRoot != null)
            smallPromptRoot.SetActive(false);

        if (bigPromptText != null)
            bigPromptText.text = $"PRESS 'TAB TO START WAVE {nextWaveNumber}";
    }

    private void ShowSmall(int nextWaveNumber)
    {
        if (bigPromptRoot != null)
            bigPromptRoot.SetActive(false);

        if (smallPromptRoot != null)
            smallPromptRoot.SetActive(true);

        if (smallPromptText != null)
            smallPromptText.text = $"TAB: START WAVE {nextWaveNumber}!";
    }
}