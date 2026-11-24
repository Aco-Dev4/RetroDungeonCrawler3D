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

    [Header("Close Confirmation")]
    [SerializeField] private ChestDiscardConfirmation discardConfirmationPrefab;

    [Header("Reward")]
    [SerializeField, Range(2, 3)] private int rewardChoiceCount = 2;
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
    private ChestDiscardConfirmation _discardConfirmation;

    private bool IsConfirmingDiscard => _discardConfirmation != null && _discardConfirmation.gameObject.activeSelf;

    private readonly List<CardUI> _spawnedCards = new();
    private readonly List<RewardOption> _currentOptions = new();
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
        if (chest == null || _currentChest != null) return;

        if (rootPanel == null || cardManager == null || cardUIPrefab == null || cardContainer == null ||
            runCardInventory == null || playerController == null || GameManager.Instance == null ||
            GameManager.Instance.State != GameState.Playing)
        {
            chest.OnRewardUIClosed();
            return;
        }

        List<RewardOption> options = cardManager.GetRandomRewardOptions(GetRewardChoiceCount());
        if (options.Count == 0)
        {
            chest.OnRewardUIClosed();
            return;
        }

        _currentChest = chest;
        SetPanelVisible(true);
        GameManager.Instance?.PauseGame();
        ShowRewardOptions(options);
        RefreshRerollUI();
    }

    public void RequestClose()
    {
        if (_currentChest == null || !IsOpen() || IsConfirmingDiscard) return;

        if (discardConfirmationPrefab == null)
        {
            Debug.LogError("Assign a discard confirmation prefab on ChestRewardUI.", this);
            return;
        }

        if (_discardConfirmation == null)
            _discardConfirmation = Instantiate(discardConfirmationPrefab, rootPanel.transform);

        _discardConfirmation.Show(Close);
    }

    public void CancelCloseConfirmation()
    {
        if (IsConfirmingDiscard)
            _discardConfirmation.Hide();
    }

    public void Close()
    {
        RewardChest chest = _currentChest;
        if (chest == null) return;

        _currentChest = null;
        CancelCloseConfirmation();
        ClearSpawnedCards();
        _currentOptions.Clear();
        SetPanelVisible(false);
        GameManager.Instance?.ResumeGame();
        chest.OnRewardUIClosed();
    }

    public void OnRerollPressed()
    {
        if (_currentChest == null || !IsOpen() || IsConfirmingDiscard) return;

        PlayerRewardEffectHandler handler = playerController.GetRewardEffectHandler();
        if (handler == null || (!handler.HasRerolls() &&
            (CurrencyManager.Instance == null || CurrencyManager.Instance.GetSilver() < handler.GetPaidRerollCost())))
        {
            AudioManager.Instance?.PlaySFX("UICancel");
            return;
        }

        List<RewardOption> options = cardManager.GetRandomRewardOptions(GetRewardChoiceCount(), _currentOptions);
        if (options.Count == 0 || (!handler.TrySpendReroll() && !handler.TryBuyPaidReroll()))
        {
            AudioManager.Instance?.PlaySFX("UICancel");
            return;
        }
        AudioManager.Instance?.PlaySFX("UIBuy");
        ShowRewardOptions(options);
        RefreshRerollUI();
    }
    #endregion

    #region Reward Display
    private int GetRewardChoiceCount()
    {
        GameDataManager data = GameDataManager.Instance;
        if (playerController.UsesPermanentUpgrades && data != null && data.HasExtraChestChoice())
            return 3;

        return rewardChoiceCount;
    }

    private void ShowRewardOptions(List<RewardOption> options)
    {
        ClearSpawnedCards();
        _currentOptions.Clear();

        _currentOptions.AddRange(options);

        foreach (RewardOption option in options)
            SpawnRewardCard(option);

        ResizePanelToCards();
    }

    private void ResizePanelToCards()
    {
        int cardCount = _spawnedCards.Count;
        if (cardCount == 0 || rootPanel.transform is not RectTransform panel ||
            !cardContainer.TryGetComponent(out HorizontalLayoutGroup row)) return;

        float cardWidth = LayoutUtility.GetPreferredWidth((RectTransform)_spawnedCards[0].transform);
        float panelWidth = row.padding.horizontal + cardWidth * cardCount + row.spacing * (cardCount - 1);
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
    }

    private void SpawnRewardCard(RewardOption rewardOption)
    {
        if (rewardOption == null || rewardOption.cardData == null)
            return;

        CardUI cardUI = Instantiate(cardUIPrefab, cardContainer);
        cardUI.Setup(rewardOption, OnRewardOptionChosen, playerController);

        _spawnedCards.Add(cardUI);
    }

    private void ClearSpawnedCards()
    {
        for (int i = _spawnedCards.Count - 1; i >= 0; i--)
        {
            if (_spawnedCards[i] != null)
            {
                _spawnedCards[i].gameObject.SetActive(false);
                Destroy(_spawnedCards[i].gameObject);
            }
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
        if (_currentChest == null || !IsOpen() || IsConfirmingDiscard || !_currentOptions.Contains(rewardOption)) return;
        if (rewardOption == null || rewardOption.cardData == null) return;
        if (runCardInventory == null || playerController == null) return;

        AudioManager.Instance?.PlaySFX("UIBuy");

        OwnedCard ownedCard = runCardInventory.GetOwnedCard(rewardOption.cardData);
        // Free upgrades retain their value for future reward weighting.
        int silverInvestment = rewardOption.isUpgrade && ownedCard != null ? ownedCard.GetUpgradeCost() : 0;
        runCardInventory.ApplyCard(rewardOption.cardData, playerController, silverInvestment);

        Close();
    }

    #endregion

    #region Rerolls
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

}
