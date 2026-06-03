using System;
using System.Collections.Generic;
using UnityEngine;

#region Helper Classes
[System.Serializable]
public class RarityLuckSettings
{
    public int peakAt = 50;
    public int peakWeight = 100;
    public int minWeight = 1;

    [Header("Curve Shape")]
    public float risePower = 1.5f;
    public float fallPower = 1.2f;
}

[Serializable]
public class CardRarityRuntimeWeight
{
    public CardRarity rarity;
    public int weight;

    public CardRarityRuntimeWeight(CardRarity rarity, int weight)
    {
        this.rarity = rarity;
        this.weight = weight;
    }
}

[Serializable]
public class UpgradeChanceSettings
{
    [Range(0f, 1f)] public float optionOneChance = 0.2f;
    [Range(0f, 1f)] public float optionTwoChance = 0.08f;
}
#endregion

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    #region References
    [Header("References")]
    [SerializeField] private CardDatabase cardDatabase;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private RunCardInventory runCardInventory;
    #endregion

    #region Upgrade Chance Settings
    [Header("Upgrade Chance By Owned Ratio")]
    [SerializeField] private UpgradeChanceSettings ownedRatioUnder20 = new() { optionOneChance = 0.15f, optionTwoChance = 0.05f };
    [SerializeField] private UpgradeChanceSettings ownedRatioUnder40 = new() { optionOneChance = 0.25f, optionTwoChance = 0.12f };
    [SerializeField] private UpgradeChanceSettings ownedRatioUnder60 = new() { optionOneChance = 0.40f, optionTwoChance = 0.25f };
    [SerializeField] private UpgradeChanceSettings ownedRatioUnder80 = new() { optionOneChance = 0.45f, optionTwoChance = 0.40f };
    [SerializeField] private UpgradeChanceSettings ownedRatioOver80 = new() { optionOneChance = 0.55f, optionTwoChance = 0.60f };

    [Header("Repeat Type Bias")]
    [SerializeField] private float repeatTypePenalty = 0.55f;
    #endregion

    #region Luck Settings
    [Header("Luck Settings")]
    [SerializeField] private int luckPerWave = 8;

    [Header("Common")]
    [SerializeField] private int commonMinWeight = 15;

    [Header("Rare Settings")]
    [SerializeField] private RarityLuckSettings rareSettings;

    [Header("Epic Settings")]
    [SerializeField] private RarityLuckSettings epicSettings;

    [Header("Legendary Settings")]
    [SerializeField] private RarityLuckSettings legendarySettings;
    #endregion

    #region Upgrade Reward Weighting
    [Header("Upgrade Reward Weighting")]
    [SerializeField] private float upgradeLevelLuckWeight = 0.015f;
    [SerializeField] private float upgradeSilverLuckWeight = 0.001f;
    [SerializeField] private float baseUpgradeOptionWeight = 100f;
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
    }
    #endregion

    #region Public Getters
    public int GetCurrentWaveLuck()
    {
        int completedWaves = waveManager != null ? waveManager.GetCompletedWaveCount() : 0;
        return Mathf.Max(0, completedWaves - 1) * luckPerWave;
    }

    public int GetCurrentEffectiveLuck()
    {
        int cardLuck = playerController != null ? playerController.GetLuck() : 0;
        return GetCurrentWaveLuck() + cardLuck;
    }

    public CardDatabase GetCardDatabase()
    {
        return cardDatabase;
    }
    #endregion

    #region Public Reward Rolling
    public List<RewardOption> GetRandomRewardOptions(int amount, List<RewardOption> blockedOptions = null)
    {
        List<RewardOption> rewardOptions = new();
        RewardOption previousOption = null;

        for (int i = 0; i < amount; i++)
        {
            RewardOption option = RollRewardOption(i, previousOption, rewardOptions, blockedOptions);

            if (option == null)
                continue;

            rewardOptions.Add(option);
            previousOption = option;
        }

        return rewardOptions;
    }

    public List<CardData> GetRandomChestCards(int amount)
    {
        List<CardData> chosenCards = new();

        for (int i = 0; i < amount; i++)
        {
            RewardOption option = GetRandomNewCardOptionOfAnyRarity(ConvertCardsToRewardOptions(chosenCards), null);

            if (option != null && option.cardData != null)
                chosenCards.Add(option.cardData);
        }

        return chosenCards;
    }
    #endregion

    #region Reward Option Rolling
    private RewardOption RollRewardOption(int optionIndex, RewardOption previousOption, List<RewardOption> currentOptions, List<RewardOption> blockedOptions)
    {
        CardRarity rolledRarity = RollAvailableRarity();

        RewardOption option = RollRewardOptionForRarity(rolledRarity, optionIndex, previousOption, currentOptions, blockedOptions);

        if (option != null)
            return option;

        return GetAnyValidRewardOption(currentOptions, blockedOptions);
    }

    private RewardOption RollRewardOptionForRarity(CardRarity rarity, int optionIndex, RewardOption previousOption, List<RewardOption> currentOptions, List<RewardOption> blockedOptions)
    {
        bool shouldUpgrade = ShouldCreateUpgradeOptionForRarity(rarity, optionIndex, previousOption);

        RewardOption option = shouldUpgrade
            ? GetRandomUpgradeOptionByRarity(rarity, currentOptions, blockedOptions)
            : GetRandomNewCardOptionByRarity(rarity, currentOptions, blockedOptions);

        if (option != null)
            return option;

        return shouldUpgrade
            ? GetRandomNewCardOptionByRarity(rarity, currentOptions, blockedOptions)
            : GetRandomUpgradeOptionByRarity(rarity, currentOptions, blockedOptions);
    }

    private bool ShouldCreateUpgradeOptionForRarity(CardRarity rarity, int optionIndex, RewardOption previousOption)
    {
        List<CardData> newCards = GetAvailableNewCardsByRarity(rarity);
        List<OwnedCard> upgradeCards = GetAvailableUpgradeCardsByRarity(rarity);

        if (upgradeCards.Count == 0)
            return false;

        if (newCards.Count == 0)
            return true;

        float ownedRatio = GetOwnedAvailableCardRatioByRarity(rarity);
        float upgradeChance = GetBaseUpgradeChance(optionIndex, ownedRatio);

        upgradeChance = ApplyRepeatTypeBias(upgradeChance, previousOption);

        return UnityEngine.Random.value < upgradeChance;
    }

    private float GetBaseUpgradeChance(int optionIndex, float ownedRatio)
    {
        UpgradeChanceSettings settings = GetUpgradeChanceSettings(ownedRatio);

        if (optionIndex == 0)
            return settings.optionOneChance;

        return settings.optionTwoChance;
    }

    private UpgradeChanceSettings GetUpgradeChanceSettings(float ownedRatio)
    {
        if (ownedRatio < 0.2f)
            return ownedRatioUnder20;

        if (ownedRatio < 0.4f)
            return ownedRatioUnder40;

        if (ownedRatio < 0.6f)
            return ownedRatioUnder60;

        if (ownedRatio < 0.8f)
            return ownedRatioUnder80;

        return ownedRatioOver80;
    }

    private float ApplyRepeatTypeBias(float upgradeChance, RewardOption previousOption)
    {
        if (previousOption == null)
            return upgradeChance;

        float repeatPenalty = repeatTypePenalty;

        if (previousOption.isUpgrade)
            return upgradeChance * repeatPenalty;

        return 1f - ((1f - upgradeChance) * repeatPenalty);
    }
    #endregion

    #region New Card Options
    private RewardOption GetRandomNewCardOptionByRarity(CardRarity rarity, List<RewardOption> excludedOptions, List<RewardOption> blockedOptions)
    {
        List<CardData> excludedCards = GetExcludedCards(excludedOptions);
        List<CardData> validCards = GetValidNewCardsByRarity(rarity, excludedCards, blockedOptions);

        if (validCards.Count == 0)
            return null;

        int index = UnityEngine.Random.Range(0, validCards.Count);
        return new RewardOption(validCards[index], false);
    }

    private RewardOption GetRandomNewCardOptionOfAnyRarity(List<RewardOption> excludedOptions, List<RewardOption> blockedOptions)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            CardRarity rarity = RollAvailableRarity();
            RewardOption option = GetRandomNewCardOptionByRarity(rarity, excludedOptions, blockedOptions);

            if (option != null)
                return option;
        }

        return GetAnyValidNewCardOption(excludedOptions, blockedOptions);
    }

    private RewardOption GetAnyValidNewCardOption(List<RewardOption> excludedOptions, List<RewardOption> blockedOptions)
    {
        List<CardData> excludedCards = GetExcludedCards(excludedOptions);
        List<CardData> validCards = new();

        if (cardDatabase == null)
            return null;

        for (int i = 0; i < cardDatabase.cards.Count; i++)
        {
            CardData card = cardDatabase.cards[i];

            if (card == null) continue;
            if (excludedCards.Contains(card)) continue;
            if (runCardInventory != null && runCardInventory.HasCard(card)) continue;
            if (!IsCardAllowed(card)) continue;

            RewardOption option = new RewardOption(card, false);
            if (IsBlockedOption(option, blockedOptions)) continue;

            validCards.Add(card);
        }

        if (validCards.Count == 0)
            return null;

        int index = UnityEngine.Random.Range(0, validCards.Count);
        return new RewardOption(validCards[index], false);
    }

    private List<CardData> GetValidNewCardsByRarity(CardRarity rarity, List<CardData> excludedCards, List<RewardOption> blockedOptions)
    {
        List<CardData> validCards = new();

        if (cardDatabase == null)
            return validCards;

        List<CardData> pool = cardDatabase.GetCardsByRarity(rarity);

        for (int i = 0; i < pool.Count; i++)
        {
            CardData card = pool[i];

            if (card == null) continue;
            if (excludedCards.Contains(card)) continue;
            if (runCardInventory != null && runCardInventory.HasCard(card)) continue;
            if (!IsCardAllowed(card)) continue;

            RewardOption option = new RewardOption(card, false);
            if (IsBlockedOption(option, blockedOptions)) continue;

            validCards.Add(card);
        }

        return validCards;
    }
    #endregion

    #region Upgrade Options
    private RewardOption GetRandomUpgradeOptionByRarity(CardRarity rarity, List<RewardOption> excludedOptions, List<RewardOption> blockedOptions)
    {
        List<OwnedCard> validCards = GetValidUpgradeCardsByRarity(rarity, excludedOptions, blockedOptions);

        if (validCards.Count == 0)
            return null;

        OwnedCard chosenCard = GetWeightedUpgradeCard(validCards);

        return chosenCard != null
            ? new RewardOption(chosenCard.cardData, true)
            : null;
    }

    private List<OwnedCard> GetValidUpgradeCardsByRarity(CardRarity rarity, List<RewardOption> excludedOptions, List<RewardOption> blockedOptions)
    {
        List<OwnedCard> validCards = new();

        if (runCardInventory == null)
            return validCards;

        for (int i = 0; i < runCardInventory.OwnedCards.Count; i++)
        {
            OwnedCard ownedCard = runCardInventory.OwnedCards[i];

            if (ownedCard == null || ownedCard.cardData == null) continue;
            if (ownedCard.cardData.rarity != rarity) continue;
            if (!IsCardAllowed(ownedCard.cardData)) continue;

            RewardOption option = new RewardOption(ownedCard.cardData, true);

            if (IsBlockedOption(option, blockedOptions)) continue;
            if (IsAlreadyChosenUpgrade(option, excludedOptions)) continue;

            validCards.Add(ownedCard);
        }

        return validCards;
    }

    private OwnedCard GetWeightedUpgradeCard(List<OwnedCard> validCards)
    {
        if (validCards == null || validCards.Count == 0)
            return null;

        int luck = GetCurrentEffectiveLuck();

        float totalWeight = 0f;
        List<float> weights = new();

        for (int i = 0; i < validCards.Count; i++)
        {
            float weight = GetUpgradeCardWeight(validCards[i], luck);
            weights.Add(weight);
            totalWeight += weight;
        }

        if (totalWeight <= 0f)
            return validCards[UnityEngine.Random.Range(0, validCards.Count)];

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float current = 0f;

        for (int i = 0; i < validCards.Count; i++)
        {
            current += weights[i];

            if (roll <= current)
                return validCards[i];
        }

        return validCards[validCards.Count - 1];
    }

    private float GetUpgradeCardWeight(OwnedCard ownedCard, int luck)
    {
        if (ownedCard == null)
            return 0f;

        float levelBonus = (ownedCard.level - 1) * luck * upgradeLevelLuckWeight;
        float silverBonus = ownedCard.silverInvested * luck * upgradeSilverLuckWeight;

        return baseUpgradeOptionWeight + levelBonus + silverBonus;
    }
    #endregion

    #region Fallback Reward Options
    private RewardOption GetAnyValidRewardOption(List<RewardOption> excludedOptions, List<RewardOption> blockedOptions)
    {
        RewardOption newOption = GetAnyValidNewCardOption(excludedOptions, blockedOptions);

        if (newOption != null)
            return newOption;

        return GetAnyValidUpgradeOption(excludedOptions, blockedOptions);
    }

    private RewardOption GetAnyValidUpgradeOption(List<RewardOption> excludedOptions, List<RewardOption> blockedOptions)
    {
        List<OwnedCard> validCards = new();

        if (runCardInventory == null)
            return null;

        for (int i = 0; i < runCardInventory.OwnedCards.Count; i++)
        {
            OwnedCard ownedCard = runCardInventory.OwnedCards[i];

            if (ownedCard == null || ownedCard.cardData == null) continue;
            if (!IsCardAllowed(ownedCard.cardData)) continue;

            RewardOption option = new RewardOption(ownedCard.cardData, true);

            if (IsBlockedOption(option, blockedOptions)) continue;
            if (IsAlreadyChosenUpgrade(option, excludedOptions)) continue;

            validCards.Add(ownedCard);
        }

        if (validCards.Count == 0)
            return null;

        OwnedCard chosenCard = GetWeightedUpgradeCard(validCards);

        return chosenCard != null
            ? new RewardOption(chosenCard.cardData, true)
            : null;
    }
    #endregion

    #region Available Pools
    private List<CardData> GetAvailableNewCardsByRarity(CardRarity rarity)
    {
        List<CardData> result = new();

        if (cardDatabase == null)
            return result;

        List<CardData> pool = cardDatabase.GetCardsByRarity(rarity);

        for (int i = 0; i < pool.Count; i++)
        {
            CardData card = pool[i];

            if (card == null) continue;
            if (!IsCardAllowed(card)) continue;
            if (runCardInventory != null && runCardInventory.HasCard(card)) continue;

            result.Add(card);
        }

        return result;
    }

    private List<OwnedCard> GetAvailableUpgradeCardsByRarity(CardRarity rarity)
    {
        List<OwnedCard> result = new();

        if (runCardInventory == null)
            return result;

        for (int i = 0; i < runCardInventory.OwnedCards.Count; i++)
        {
            OwnedCard ownedCard = runCardInventory.OwnedCards[i];

            if (ownedCard == null || ownedCard.cardData == null) continue;
            if (ownedCard.cardData.rarity != rarity) continue;
            if (!IsCardAllowed(ownedCard.cardData)) continue;

            result.Add(ownedCard);
        }

        return result;
    }

    private float GetOwnedAvailableCardRatioByRarity(CardRarity rarity)
    {
        List<CardData> newCards = GetAvailableNewCardsByRarity(rarity);
        List<OwnedCard> upgradeCards = GetAvailableUpgradeCardsByRarity(rarity);

        int ownedCount = upgradeCards.Count;
        int totalAvailableCount = ownedCount + newCards.Count;

        if (totalAvailableCount <= 0)
            return 0f;

        return (float)ownedCount / totalAvailableCount;
    }
    #endregion

    #region Rarity Rolling
    private CardRarity RollAvailableRarity()
    {
        int effectiveLuck = GetCurrentEffectiveLuck();
        List<CardRarityRuntimeWeight> weights = BuildAvailableRarityWeights(effectiveLuck);

        int totalWeight = GetTotalRarityWeight(weights);

        if (totalWeight <= 0)
            return CardRarity.COMMON;

        int roll = UnityEngine.Random.Range(0, totalWeight);
        int current = 0;

        for (int i = 0; i < weights.Count; i++)
        {
            current += weights[i].weight;

            if (roll < current)
                return weights[i].rarity;
        }

        return CardRarity.COMMON;
    }

    private List<CardRarityRuntimeWeight> BuildAvailableRarityWeights(int luck)
    {
        List<CardRarityRuntimeWeight> result = new();

        AddWeightIfAvailable(result, CardRarity.COMMON, GetCommonWeight(luck));
        AddWeightIfAvailable(result, CardRarity.RARE, GetRareWeight(luck));
        AddWeightIfAvailable(result, CardRarity.EPIC, GetEpicWeight(luck));
        AddWeightIfAvailable(result, CardRarity.LEGENDARY, GetLegendaryWeight(luck));

        return result;
    }

    private void AddWeightIfAvailable(List<CardRarityRuntimeWeight> list, CardRarity rarity, int weight)
    {
        if (weight <= 0) return;
        if (!HasAnyAllowedCardOrUpgradeByRarity(rarity)) return;

        list.Add(new CardRarityRuntimeWeight(rarity, weight));
    }

    private bool HasAnyAllowedCardOrUpgradeByRarity(CardRarity rarity)
    {
        if (GetAvailableNewCardsByRarity(rarity).Count > 0)
            return true;

        if (GetAvailableUpgradeCardsByRarity(rarity).Count > 0)
            return true;

        return false;
    }

    private int GetTotalRarityWeight(List<CardRarityRuntimeWeight> weights)
    {
        int totalWeight = 0;

        for (int i = 0; i < weights.Count; i++)
            totalWeight += weights[i].weight;

        return totalWeight;
    }
    #endregion

    #region Rarity Weight Calculations
    private int GetCommonWeight(int luck)
    {
        float t = Mathf.Clamp01(luck / Mathf.Max(1f, rareSettings.peakAt));
        float weight = Mathf.Lerp(100f, commonMinWeight, Mathf.Pow(t, 1.2f));

        return Mathf.RoundToInt(weight);
    }

    private int GetRareWeight(int luck)
    {
        return GetPeakWeight(luck, rareSettings);
    }

    private int GetEpicWeight(int luck)
    {
        if (GameDataManager.Instance == null || !GameDataManager.Instance.HasUnlockedEpicCards())
            return 0;

        return GetPeakWeight(luck, epicSettings);
    }

    private int GetLegendaryWeight(int luck)
    {
        if (GameDataManager.Instance == null || !GameDataManager.Instance.HasUnlockedLegendaryCards())
            return 0;

        float t = Mathf.Clamp01(luck / Mathf.Max(1f, legendarySettings.peakAt));
        float weight = Mathf.Lerp(legendarySettings.minWeight, legendarySettings.peakWeight, Mathf.Pow(t, legendarySettings.risePower));

        return Mathf.RoundToInt(weight);
    }

    private int GetPeakWeight(int luck, RarityLuckSettings settings)
    {
        if (settings == null)
            return 0;

        float peak = Mathf.Max(1f, settings.peakAt);

        if (luck <= peak)
        {
            float t = Mathf.Clamp01(luck / peak);
            float weight = Mathf.Lerp(settings.minWeight, settings.peakWeight, Mathf.Pow(t, settings.risePower));

            return Mathf.RoundToInt(weight);
        }

        float fallT = Mathf.Clamp01((luck - peak) / peak);
        float fallWeight = Mathf.Lerp(settings.peakWeight, settings.minWeight, Mathf.Pow(fallT, settings.fallPower));

        return Mathf.RoundToInt(fallWeight);
    }
    #endregion

    #region Card Rules
    private bool IsCardAllowed(CardData card)
    {
        if (card == null) return false;
        if (!card.canAppearInChest) return false;

        if (card.rarity == CardRarity.EPIC &&
            (GameDataManager.Instance == null || !GameDataManager.Instance.HasUnlockedEpicCards()))
            return false;

        if (card.rarity == CardRarity.LEGENDARY &&
            (GameDataManager.Instance == null || !GameDataManager.Instance.HasUnlockedLegendaryCards()))
            return false;

        if (card.requiresWeapon &&
            (GameDataManager.Instance == null || !GameDataManager.Instance.HasSword()))
            return false;

        if (card.requiresCriticalHits &&
            (GameDataManager.Instance == null || !GameDataManager.Instance.HasUnlockedCriticalHits()))
            return false;

        return true;
    }
    #endregion

    #region Helpers
    private bool IsBlockedOption(RewardOption option, List<RewardOption> blockedOptions)
    {
        if (option == null || blockedOptions == null)
            return false;

        for (int i = 0; i < blockedOptions.Count; i++)
        {
            if (option.Matches(blockedOptions[i]))
                return true;
        }

        return false;
    }

    private bool IsAlreadyChosenUpgrade(RewardOption option, List<RewardOption> chosenOptions)
    {
        if (option == null || chosenOptions == null)
            return false;

        for (int i = 0; i < chosenOptions.Count; i++)
        {
            RewardOption chosenOption = chosenOptions[i];

            if (chosenOption == null) continue;
            if (!chosenOption.isUpgrade) continue;

            if (option.cardData == chosenOption.cardData)
                return true;
        }

        return false;
    }

    private List<CardData> GetExcludedCards(List<RewardOption> rewardOptions)
    {
        List<CardData> excludedCards = new();

        if (rewardOptions == null)
            return excludedCards;

        for (int i = 0; i < rewardOptions.Count; i++)
        {
            if (rewardOptions[i] != null && rewardOptions[i].cardData != null)
                excludedCards.Add(rewardOptions[i].cardData);
        }

        return excludedCards;
    }

    private List<RewardOption> ConvertCardsToRewardOptions(List<CardData> cards)
    {
        List<RewardOption> options = new();

        if (cards == null)
            return options;

        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
                options.Add(new RewardOption(cards[i], false));
        }

        return options;
    }
    #endregion
}