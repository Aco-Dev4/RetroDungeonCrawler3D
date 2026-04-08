using System.Collections.Generic;
using UnityEngine;

public class ChestRewardUI : MonoBehaviour
{
    public static ChestRewardUI Instance;

    #region References
    [Header("References")]
    [SerializeField] private GameObject rootPanel;
    [SerializeField] private CardUI cardUIPrefab;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private RunCardInventory runCardInventory;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private CardManager cardManager;
    #endregion

    #region Runtime
    private RewardChest _currentChest;
    private readonly List<CardUI> _spawnedCards = new();
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        rootPanel.SetActive(false);
    }

    public void Open(RewardChest chest)
    {
        _currentChest = chest;
        rootPanel.SetActive(true);
        GameManager.Instance.PauseGame();
        ShowRandomCards(2);
    }

    public void Close()
    {
        ClearSpawnedCards();
        rootPanel.SetActive(false);
        GameManager.Instance.ResumeGame();

        if (_currentChest != null)
        {
            _currentChest.OnRewardUIClosed();
            _currentChest = null;
        }
    }

    #region Card Display
    private void ShowRandomCards(int amount)
    {
        ClearSpawnedCards();

        if (cardManager == null || cardUIPrefab == null || cardContainer == null) return;

        List<CardData> chosenCards = cardManager.GetRandomChestCards(amount);

        for (int i = 0; i < chosenCards.Count; i++)
        {
            CardUI cardUI = Instantiate(cardUIPrefab, cardContainer);
            cardUI.Setup(chosenCards[i], OnCardChosen);
            _spawnedCards.Add(cardUI);
        }
    }

    private void ClearSpawnedCards()
    {
        for (int i = _spawnedCards.Count - 1; i >= 0; i--)
        {
            if (_spawnedCards[i] != null)
                Destroy(_spawnedCards[i].gameObject);
        }

        _spawnedCards.Clear();
    }
    #endregion

    #region Card Choosing
    private void OnCardChosen(CardData cardData)
    {
        if (cardData == null || runCardInventory == null || playerController == null) return;

        bool alreadyOwned = runCardInventory.HasCard(cardData);

        if (!alreadyOwned)
            runCardInventory.AddCard(cardData);
        else
            runCardInventory.UpgradeCard(cardData);

        playerController.RecalculateStats();
        ApplyInstantCardEffect(cardData, alreadyOwned);
        Close();
    }

    private void ApplyInstantCardEffect(CardData cardData, bool wasUpgrade)
    {
        if (cardData == null || playerController == null) return;

        Health health = playerController.GetComponent<Health>();
        if (health == null) return;

        switch (cardData.statType)
        {
            case CardStatType.Heal:
                health.HealPercent(wasUpgrade ? cardData.valuePerUpgrade : cardData.baseValue);
                break;
        }
    }
    #endregion
}