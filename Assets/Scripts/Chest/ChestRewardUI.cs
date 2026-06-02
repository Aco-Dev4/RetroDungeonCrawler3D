using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    #region Reroll UI
    [Header("Reroll")]
    [SerializeField] private Button rerollButton;
    [SerializeField] private TMP_Text rerollCountText;

    [Header("Reroll Visuals")]
    [SerializeField] private Image rerollButtonImage;
    [SerializeField] private GameObject rerollSilverIcon;
    [SerializeField] private Color canAffordRerollColor = Color.blue;
    [SerializeField] private Color cannotAffordRerollColor = Color.red;
    [SerializeField] private Color freeRerollColor = Color.green;
    #endregion

    #region Runtime
    private RewardChest _currentChest;

    private readonly List<CardUI> _spawnedCards = new();
    private readonly List<RewardOption> _currentOptions = new();
    private readonly List<RewardOption> _previousOptions = new();
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

    public void Open(RewardChest chest)
    {
        _currentChest = chest;

        SetPanelVisible(true);
        GameManager.Instance?.PauseGame();

        _previousOptions.Clear();

        ShowRewardOptions(2);
        RefreshRerollUI();
    }

    public void Close()
    {
        ClearSpawnedCards();
        SetPanelVisible(false);
        GameManager.Instance?.ResumeGame();

        if (_currentChest != null)
        {
            _currentChest.OnRewardUIClosed();
            _currentChest = null;
        }
    }

    public void OnRerollPressed()
    {
        if (!TryPayForReroll())
            return;

        _previousOptions.Clear();
        _previousOptions.AddRange(_currentOptions);

        ShowRewardOptions(2);
        RefreshRerollUI();
    }
    #endregion

    #region Reward Display
    private void ShowRewardOptions(int amount)
    {
        ClearSpawnedCards();
        _currentOptions.Clear();

        if (cardManager == null || cardUIPrefab == null || cardContainer == null)
            return;

        List<RewardOption> rewardOptions = cardManager.GetRandomRewardOptions(amount, _previousOptions);
        _currentOptions.AddRange(rewardOptions);

        for (int i = 0; i < rewardOptions.Count; i++)
            SpawnRewardCard(rewardOptions[i]);
    }

    private void SpawnRewardCard(RewardOption rewardOption)
    {
        if (rewardOption == null || rewardOption.cardData == null)
            return;

        CardUI cardUI = Instantiate(cardUIPrefab, cardContainer);
        cardUI.Setup(rewardOption, OnRewardOptionChosen);

        _spawnedCards.Add(cardUI);
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

    private void SetPanelVisible(bool visible)
    {
        if (rootPanel != null)
            rootPanel.SetActive(visible);
    }
    #endregion

    #region Choosing Rewards
    private void OnRewardOptionChosen(RewardOption rewardOption)
    {
        if (rewardOption == null || rewardOption.cardData == null) return;
        if (runCardInventory == null || playerController == null) return;

        if (rewardOption.isUpgrade)
            ApplyUpgradeReward(rewardOption.cardData);
        else
            ApplyNewCardReward(rewardOption.cardData);

        Close();
    }

    private void ApplyNewCardReward(CardData cardData)
    {
        if (cardData == null) return;

        bool alreadyOwned = runCardInventory.HasCard(cardData);

        if (!alreadyOwned)
            runCardInventory.AddCard(cardData);
        else
            runCardInventory.UpgradeCard(cardData);

        playerController.RecalculateStats();
        ApplyInstantCardEffect(cardData, alreadyOwned);
    }

    private void ApplyUpgradeReward(CardData cardData)
    {
        if (cardData == null) return;

        OwnedCard ownedCard = runCardInventory.GetOwnedCard(cardData);
        int fakeInvestment = ownedCard != null ? GetUpgradeCost(ownedCard) : 0;

        runCardInventory.UpgradeCard(cardData, fakeInvestment);
        RunStatsManager.Instance?.AddCardUpgraded();

        playerController.RecalculateStats();
        ApplyInstantCardEffect(cardData, true);
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

    #region Rerolls
    private bool TryPayForReroll()
    {
        if (playerController == null) return false;

        PlayerRewardEffectHandler rewardHandler = playerController.GetRewardEffectHandler();
        if (rewardHandler == null) return false;

        if (rewardHandler.TrySpendReroll())
            return true;

        return rewardHandler.TryBuyPaidReroll();
    }

    private void RefreshRerollUI()
    {
        PlayerRewardEffectHandler rewardHandler = playerController != null ? playerController.GetRewardEffectHandler() : null;
        if (rewardHandler == null) return;

        int freeRerolls = rewardHandler.RewardRerolls;

        if (freeRerolls > 0)
        {
            ShowFreeRerollState(freeRerolls);
            return;
        }

        ShowPaidRerollState(rewardHandler);
    }

    private void ShowFreeRerollState(int freeRerolls)
    {
        if (rerollCountText != null)
            rerollCountText.text = freeRerolls.ToString();

        if (rerollSilverIcon != null)
            rerollSilverIcon.SetActive(false);

        if (rerollButtonImage != null)
            rerollButtonImage.color = freeRerollColor;

        if (rerollButton != null)
            rerollButton.interactable = true;
    }

    private void ShowPaidRerollState(PlayerRewardEffectHandler rewardHandler)
    {
        int cost = rewardHandler.GetPaidRerollCost();
        int currentSilver = CurrencyManager.Instance != null ? CurrencyManager.Instance.GetSilver() : 0;
        bool canAfford = currentSilver >= cost;

        if (rerollCountText != null)
            rerollCountText.text = cost.ToString();

        if (rerollSilverIcon != null)
            rerollSilverIcon.SetActive(true);

        if (rerollButtonImage != null)
            rerollButtonImage.color = canAfford ? canAffordRerollColor : cannotAffordRerollColor;

        if (rerollButton != null)
            rerollButton.interactable = true;
    }
    #endregion

    #region Costs
    private int GetUpgradeCost(OwnedCard ownedCard)
    {
        if (ownedCard == null || ownedCard.cardData == null)
            return 0;

        return ownedCard.cardData.baseUpgradeCost + ownedCard.cardData.costIncreasePerLevel * (ownedCard.level - 1);
    }
    #endregion
}