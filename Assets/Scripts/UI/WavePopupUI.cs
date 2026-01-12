using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class WavePopupUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeInTime = 0.3f;
    [SerializeField] private float stayTime = 1.5f;
    [SerializeField] private float fadeOutTime = 0.3f;

    [Header("Colors")]
    [SerializeField] private Color waveStartedColor = Color.green;
    [SerializeField] private Color waveCompletedColor = Color.yellow;

    private Coroutine _currentRoutine;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Show(string message, WavePopupType type)
    {
        if (_currentRoutine != null)
            StopCoroutine(_currentRoutine);

        SetStyle(type);

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        _currentRoutine = StartCoroutine(ShowRoutine(message));
    }

    private void SetStyle(WavePopupType type)
    {
        switch (type)
        {
            case WavePopupType.WaveStarted:
                backgroundImage.color = waveStartedColor;
                break;

            case WavePopupType.WaveCompleted:
                backgroundImage.color = waveCompletedColor;
                break;
        }
    }

    private IEnumerator ShowRoutine(string message)
    {
        popupText.text = message;

        // SFX goes here

        yield return Fade(0f, 1f, fadeInTime);
        yield return new WaitForSeconds(stayTime);
        yield return Fade(1f, 0f, fadeOutTime);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator Fade(float from, float to, float time)
    {
        float t = 0f;
        canvasGroup.alpha = from;

        while (t < time)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, t / time);
            yield return null;
        }

        canvasGroup.alpha = to;
    }
}

