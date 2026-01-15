using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Currencies")]
    [SerializeField] private int gold;
    [SerializeField] private int silver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // ===== Silver =====
    public void AddSilver(int amount)
    {
        silver += amount;
        // UI update goes here
        // SFX goes here
    }

    public int GetSilver() => silver;

    // ===== Gold (already planned) =====
    public void AddGold(int amount)
    {
        gold += amount;
        // UI update goes here
        // SFX goes here
        GoldUI.Instance?.SetGold(gold);
    }

    public int GetGold() => gold;
}


