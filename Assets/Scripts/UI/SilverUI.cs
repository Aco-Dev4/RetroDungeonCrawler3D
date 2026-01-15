using TMPro;
using UnityEngine;

public class SilverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI silverText;

    private void Update()
    {
        if (CurrencyManager.Instance == null) return;
        silverText.text = CurrencyManager.Instance.GetSilver().ToString();
    }
}

