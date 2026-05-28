using TMPro;
using UnityEngine;

public class RunTimer : MonoBehaviour
{
    public static RunTimer Instance;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;

    private float _elapsedTime;
    private bool _isRunning;

    public float ElapsedTime => _elapsedTime;

    private void Awake()
    {
        Instance = this;
        UpdateTimerText();
    }

    private void Start()
    {
        StartTimer();
    }

    private void Update()
    {
        if (!_isRunning) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.State != GameState.Playing) return;

        _elapsedTime += Time.deltaTime;
        UpdateTimerText();
    }

    public void StartTimer()
    {
        _elapsedTime = 0f;
        _isRunning = true;
        UpdateTimerText();
    }

    public void StopTimer()
    {
        _isRunning = false;
        UpdateTimerText();
    }

    public string GetFormattedTime()
    {
        return FormatTime(_elapsedTime);
    }

    private void UpdateTimerText()
    {
        if (timerText == null) return;

        timerText.text = $"TIMER:\n{FormatTime(_elapsedTime)}";
    }

    private string FormatTime(float time)
    {
        int hours = Mathf.FloorToInt(time / 3600f);
        int minutes = Mathf.FloorToInt((time % 3600f) / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int centiseconds = Mathf.FloorToInt((time * 100f) % 100f);

        if (hours > 0)
            return $"{hours}:{minutes:00}:{seconds:00}.{centiseconds:00}";

        if (minutes > 0)
            return $"{minutes}:{seconds:00}.{centiseconds:00}";

        return $"{seconds}.{centiseconds:00}";
    }
}