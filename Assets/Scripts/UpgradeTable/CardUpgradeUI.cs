using System.Collections.Generic;
using UnityEngine;

public class CardUpgradeUI : MonoBehaviour
{
    public static CardUpgradeUI Instance;

    [Header("References")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private UpgradeCardSlotUI slotPrefab;
    [SerializeField] private RunCardInventory runCardInventory;
    [SerializeField] private PlayerController playerController;

    [Header("Tutorial")]
    [SerializeField] private bool useTutorialUpgradeCost;
    [SerializeField] private int tutorialUpgradeCost = 1;

    private readonly List<UpgradeCardSlotUI> _spawnedSlots = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (rootPanel != null)
            rootPanel.SetActive(false);
    }

    public bool IsOpen()
    {
        return rootPanel != null && rootPanel.activeSelf;
    }

    public void Open()
    {
        if (rootPanel == null) return;

        rootPanel.SetActive(true);
        GameManager.Instance?.PauseGame();
        Refresh();
    }

    public void Close()
    {
        ClearSlots();

        if (rootPanel != null)
            rootPanel.SetActive(false);

        GameManager.Instance?.ResumeGame();
        WaveManager.Instance?.TryShowTutorialFinish();
    }

    private void Refresh()
    {
        ClearSlots();

        if (runCardInventory == null || slotPrefab == null || cardContainer == null) return;

        foreach (OwnedCard ownedCard in runCardInventory.OwnedCards)
        {
            if (ownedCard == null || ownedCard.cardData == null) continue;

            int cost = GetUpgradeCost(ownedCard);
            UpgradeCardSlotUI slot = Instantiate(slotPrefab, cardContainer);
            int currentSilver = CurrencyManager.Instance != null ? CurrencyManager.Instance.GetSilver() : 0;
            slot.Setup(ownedCard, cost, currentSilver, playerController, TryUpgradeCard);
            _spawnedSlots.Add(slot);
        }
    }

    private void TryUpgradeCard(OwnedCard ownedCard)
    {
        if (ownedCard == null || ownedCard.cardData == null) return;

        int cost = GetUpgradeCost(ownedCard);

        if (CurrencyManager.Instance == null) return;
        if (CurrencyManager.Instance.GetSilver() < cost)
        {
            Debug.Log("Not enough silver.");
            return;
        }

        CurrencyManager.Instance.SetSilver(CurrencyManager.Instance.GetSilver() - cost);

        CardData cardData = ownedCard.cardData;
        runCardInventory.UpgradeCard(cardData);
        RunStatsManager.Instance?.AddCardUpgraded();
        playerController.RecalculateStats();

        ApplyUpgradeInstantEffect(cardData);

        Refresh();
        WaveManager.Instance?.OnTutorialUpgradeCompleted();
    }

    private int GetUpgradeCost(OwnedCard ownedCard)
    {
        if (ownedCard == null || ownedCard.cardData == null) return 0;

        if (useTutorialUpgradeCost)
            return tutorialUpgradeCost;

        return ownedCard.cardData.baseUpgradeCost + ownedCard.cardData.costIncreasePerLevel * (ownedCard.level - 1);
    }

    private void ApplyUpgradeInstantEffect(CardData cardData)
    {
        if (cardData == null || playerController == null) return;

        switch (cardData.statType)
        {
            case CardStatType.Heal:
                Health health = playerController.GetComponent<Health>();
                if (health != null)
                    health.HealPercent(cardData.valuePerUpgrade);
                break;
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
}