using System;
using System.Collections.Generic;
using UnityEngine;

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
        if (settings == null) return 0;

        float peak = Mathf.Max(1f, settings.peakAt);

        if (luck <= peak)
        {
            float t = Mathf.Clamp01(luck / peak);
            float weight = Mathf.Lerp(settings.minWeight, settings.peakWeight, Mathf.Pow(t, settings.risePower));
            return Mathf.RoundToInt(weight);
        }
        else
        {
            float t = Mathf.Clamp01((luck - peak) / peak);
            float weight = Mathf.Lerp(settings.peakWeight, settings.minWeight, Mathf.Pow(t, settings.fallPower));
            return Mathf.RoundToInt(weight);
        }
    }
    #endregion
}