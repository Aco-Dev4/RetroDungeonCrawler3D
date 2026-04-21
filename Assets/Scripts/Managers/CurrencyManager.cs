using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance;

    [Header("Runtime")]
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

    public int GetSilver()
    {
        return silver;
    }

    public void SetSilver(int value)
    {
        silver = Mathf.Max(0, value);
    }

    // ===== Gold =====
    public void AddGold(int amount)
    {
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.AddGold(amount);

        GoldUI.Instance?.SetGold(GetGold());
    }

    public int GetGold()
    {
        if (GameDataManager.Instance != null)
            return GameDataManager.Instance.GetGold();

        return 0;
    }

    public void SetGold(int value)
    {
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SetGold(value);

        GoldUI.Instance?.SetGold(GetGold());
    }

    private void Start()
    {
        GoldUI.Instance?.SetGold(GetGold());
    }
}