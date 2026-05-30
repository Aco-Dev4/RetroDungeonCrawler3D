using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RarityLuckSettings
{
    public int unlockAt;
    public int growthMultiplier = 1;
    public int maxWeight = 75;

    [Header("Late Game Falloff")]
    public int falloffStart = -1;
    public int falloffMin = 0;
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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #region Public Getters
    public int GetCurrentWaveLuck()
    {
        int completedWaves = waveManager != null ? waveManager.GetCompletedWaveCount() : 0;
        return Mathf.Max(0, completedWaves - 1) * luckPerWave;
    }

    public int GetCurrentEffectiveLuck()
    {
        return GetCurrentWaveLuck() + (playerController != null ? playerController.GetLuck() : 0);
    }

    public CardDatabase GetCardDatabase()
    {
        return cardDatabase;
    }
    #endregion

    #region Card Rolling
    public List<CardData> GetRandomChestCards(int amount)
    {
        List<CardData> chosenCards = new();

        if (cardDatabase == null) return chosenCards;

        for (int i = 0; i < amount; i++)
        {
            CardData chosenCard = null;

            for (int attempt = 0; attempt < 10; attempt++)
            {
                CardRarity rolledRarity = RollAvailableRarity();
                chosenCard = GetRandomCardByRarity(rolledRarity, chosenCards);

                if (chosenCard != null)
                {
                    Debug.Log($"Rolled rarity for slot {i + 1}: {rolledRarity}");
                    break;
                }
            }

            if (chosenCard == null)
                chosenCard = GetAnyValidCard(chosenCards);

            if (chosenCard != null)
                chosenCards.Add(chosenCard);
        }

        return chosenCards;
    }

    private CardData GetRandomCardByRarity(CardRarity rarity, List<CardData> excludedCards)
    {
        List<CardData> pool = cardDatabase.GetCardsByRarity(rarity);
        List<CardData> validPool = new();

        for (int i = 0; i < pool.Count; i++)
        {
            CardData card = pool[i];

            if (card == null) continue;
            if (excludedCards.Contains(card)) continue;
            if (runCardInventory != null && runCardInventory.HasCard(card)) continue;
            if (!IsCardAllowed(card)) continue;

            validPool.Add(card);
        }

        if (validPool.Count == 0)
            return null;

        int index = UnityEngine.Random.Range(0, validPool.Count);
        return validPool[index];
    }

    private CardRarity RollAvailableRarity()
    {
        int effectiveLuck = GetCurrentEffectiveLuck();
        List<CardRarityRuntimeWeight> weights = BuildAvailableRarityWeights(effectiveLuck);
        int totalWeight = 0;

        Debug.Log($"Wave Luck = {GetCurrentWaveLuck()}");
        Debug.Log($"Card Luck = {(playerController != null ? playerController.GetLuck() : 0)}");
        Debug.Log($"Effective Luck = {effectiveLuck}");

        for (int i = 0; i < weights.Count; i++)
        {
            totalWeight += weights[i].weight;
            Debug.Log($"{weights[i].rarity} weight = {weights[i].weight}");
        }

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

    private CardData GetAnyValidCard(List<CardData> excludedCards)
    {
        List<CardData> validPool = new();

        for (int i = 0; i < cardDatabase.cards.Count; i++)
        {
            CardData card = cardDatabase.cards[i];

            if (card == null) continue;
            if (excludedCards.Contains(card)) continue;
            if (runCardInventory != null && runCardInventory.HasCard(card)) continue;
            if (!IsCardAllowed(card)) continue;

            validPool.Add(card);
        }

        if (validPool.Count == 0)
            return null;

        int index = UnityEngine.Random.Range(0, validPool.Count);
        return validPool[index];
    }

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
        if (cardDatabase == null) return;
        if (cardDatabase.GetCardsByRarity(rarity).Count == 0) return;

        list.Add(new CardRarityRuntimeWeight(rarity, weight));
    }
    #endregion

    #region Weight Calculations
    private int GetCommonWeight(int luck)
    {
        return Mathf.Max(100 - luck, commonMinWeight);
    }

    private int GetRareWeight(int luck)
    {
        if (luck < rareSettings.unlockAt) return 0;

        int value = (luck - rareSettings.unlockAt) * rareSettings.growthMultiplier;
        value = Mathf.Min(value, rareSettings.maxWeight);

        if (rareSettings.falloffStart > 0 && luck > rareSettings.falloffStart)
        {
            int falloff = luck - rareSettings.falloffStart;
            value = Mathf.Max(value - falloff, rareSettings.falloffMin);
        }

        return value;
    }

    private int GetEpicWeight(int luck)
    {
        if (luck < epicSettings.unlockAt) return 0;

        int value = (luck - epicSettings.unlockAt) * epicSettings.growthMultiplier;
        value = Mathf.Min(value, epicSettings.maxWeight);

        if (epicSettings.falloffStart > 0 && luck > epicSettings.falloffStart)
        {
            int falloff = luck - epicSettings.falloffStart;
            value = Mathf.Max(value - falloff, epicSettings.falloffMin);
        }

        return value;
    }

    private int GetLegendaryWeight(int luck)
    {
        if (luck < legendarySettings.unlockAt) return 0;

        int value = (luck - legendarySettings.unlockAt) * legendarySettings.growthMultiplier;
        return value;
    }
    #endregion
}