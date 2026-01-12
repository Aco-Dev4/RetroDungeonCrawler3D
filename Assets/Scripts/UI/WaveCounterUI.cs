using UnityEngine;
using TMPro;

public class WaveCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;

    public void SetWave(int currentWave, int totalWaves)
    {
        counterText.text = $"WAVE:\n{currentWave}/{totalWaves}";
    }
}

