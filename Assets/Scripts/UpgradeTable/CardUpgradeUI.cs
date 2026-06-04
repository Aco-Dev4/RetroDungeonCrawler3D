using System.Collections.Generic;
using UnityEngine;

public class CardUpgradeUI : MonoBehaviour
{
    public static CardUpgradeUI Instance;

    #region References
    [Header("References")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private UpgradeCardSlotUI slotPrefab;
    [SerializeField] private RunCardInventory runCardInventory;
    [SerializeField] private PlayerController playerController;
    #endregion

    #region Tutorial
    [Header("Tutorial")]
    [SerializeField] private bool useTutorialUpgradeCost;
    [SerializeField] private int tutorialUpgradeCost = 1;
    #endregion

    #region Runtime
    private readonly List<UpgradeCardSlotUI> _spawnedSlots = new();
    #endregion

    #region Unity
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SetPanelVisible(false);
    }
    #endregion

    #region Public
    public bool IsOpen()
    {
        return rootPanel != null && rootPanel.activeSelf;
    }

    public void Open()
    {
        if (rootPanel == null) return;

        SetPanelVisible(true);
        GameManager.Instance?.PauseGame();
        Refresh();
    }

    public void Close()
    {
        ClearSlots();
        SetPanelVisible(false);

        GameManager.Instance?.ResumeGame();
        WaveManager.Instance?.TryShowTutorialFinish();
    }
    #endregion

    #region Display
    private void Refresh()
    {
        ClearSlots();

        if (runCardInventory == null || slotPrefab == null || cardContainer == null)
            return;

        int currentSilver = CurrencyManager.Instance != null ? CurrencyManager.Instance.GetSilver() : 0;

        foreach (OwnedCard ownedCard in runCardInventory.OwnedCards)
        {
            if (ownedCard == null || ownedCard.cardData == null) continue;

            int cost = GetUpgradeCost(ownedCard);

            UpgradeCardSlotUI slot = Instantiate(slotPrefab, cardContainer);
            slot.Setup(ownedCard, cost, currentSilver, playerController, TryUpgradeCard);

            _spawnedSlots.Add(slot);
        }
    }

    private void ClearSlots()
    {
        for (int i = _spawnedSlots.Count - 1; i >= 0; i--)
        {
            if (_spawnedSlots[i] != null)
                Destroy(_spawnedSlots[i].gameObject);
        }

        _spawnedSlots.Clear();
    }

    private void SetPanelVisible(bool visible)
    {
        if (rootPanel != null)
            rootPanel.SetActive(visible);
    }
    #endregion

    #region Upgrade Logic
    private void TryUpgradeCard(OwnedCard ownedCard)
    {
        if (ownedCard == null || ownedCard.cardData == null) return;
        if (CurrencyManager.Instance == null) return;
        if (runCardInventory == null || playerController == null) return;

        int cost = GetUpgradeCost(ownedCard);

        if (CurrencyManager.Instance.GetSilver() < cost)
        {
            AudioManager.Instance?.PlaySFX("UICancel");
            return;
        }

        CurrencyManager.Instance.SetSilver(CurrencyManager.Instance.GetSilver() - cost);
        AudioManager.Instance?.PlaySFX("UIBuy");

        CardData cardData = ownedCard.cardData;

        runCardInventory.UpgradeCard(cardData, cost);
        RunStatsManager.Instance?.AddCardUpgraded();
        playerController.RecalculateStats();

        ApplyInstantUpgradeEffect(cardData);

        Refresh();
        WaveManager.Instance?.OnTutorialUpgradeCompleted();
    }

    private int GetUpgradeCost(OwnedCard ownedCard)
    {
        if (ownedCard == null || ownedCard.cardData == null)
            return 0;

        if (useTutorialUpgradeCost)
            return tutorialUpgradeCost;

        return ownedCard.cardData.baseUpgradeCost + ownedCard.cardData.costIncreasePerLevel * (ownedCard.level - 1);
    }

    private void ApplyInstantUpgradeEffect(CardData cardData)
    {
        if (cardData == null || playerController == null) return;

        switch (cardData.statType)
        {
            case CardStatType.Heal:
                Health health = playerController.GetComponent<Health>();
                health?.HealPercent(cardData.valuePerUpgrade);
                break;
        }
    }
    #endregion
}