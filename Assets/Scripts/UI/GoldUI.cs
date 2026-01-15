using UnityEngine;
using TMPro;

public class GoldUI : MonoBehaviour
{
    public static GoldUI Instance;

    [SerializeField] private TMP_Text goldText;

    private void Awake()
    {
        Instance = this;
    }

    public void SetGold(int amount)
    {
        goldText.text = amount.ToString();
    }
}

