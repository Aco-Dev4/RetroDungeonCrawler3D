using UnityEngine;
using TMPro;

public class WaveCounterUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI counterText;

    public void SetWave(int currentWave, int totalWaves, int activeWaves)
    {
        if (counterText == null) return;

        string waveLabel = $"WAVE:\n{currentWave}/{totalWaves}";
        counterText.text = activeWaves > 1
            ? $"{waveLabel}\n<size=12><color=#FFD363>{activeWaves} WAVES ACTIVE</color></size>"
            : waveLabel;
    }
}

