using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCardSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image cardIcon;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image background;
    [SerializeField] private List<CardRarityColor> rarityColors = new();
    [SerializeField] private Image upgradeButtonImage;
    [SerializeField] private Color canAffordColor = Color.green;
    [SerializeField] private Color cannotAffordColor = Color.red;

    private OwnedCard _ownedCard;
    private Action<OwnedCard> _onUpgradePressed;

    public void Setup(OwnedCard ownedCard, int upgradeCost, int currentSilver, PlayerController playerController, Action<OwnedCard> onUpgradePressed)
    {
        _ownedCard = ownedCard;
        _onUpgradePressed = onUpgradePressed;

        if (ownedCard == null || ownedCard.cardData == null)
            return;

        bool canAfford = currentSilver >= upgradeCost;

        if (upgradeButtonImage != null)
            upgradeButtonImage.color = canAfford ? canAffordColor : cannotAffordColor;

        CardData cardData = ownedCard.cardData;
        CardRarityColor rarityColor = GetRarityColor(cardData.rarity);
        if (rarityColor != null && background != null)
            background.color = rarityColor.backgroundColor;

        cardNameText.text = cardData.cardName;
        levelText.text = $"LEVEL: {ownedCard.level}";
        costText.text = upgradeCost.ToString();

        if (cardIcon != null)
        {
            cardIcon.sprite = cardData.icon;
            cardIcon.enabled = cardData.icon != null;
        }

        valueText.text = GetUpgradePreviewText(ownedCard, playerController);

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() => _onUpgradePressed?.Invoke(_ownedCard));
    }

    private string GetUpgradePreviewText(OwnedCard ownedCard, PlayerController playerController)
    {
        if (ownedCard == null || ownedCard.cardData == null)
            return "";

        CardData cardData = ownedCard.cardData;

        Health health = playerController != null ? playerController.GetComponent<Health>() : null;

        if (cardData.statType == CardStatType.Heal)
        {
            if (health == null)
                return $"Heal {cardData.valuePerUpgrade * 100f:0}%";

            int currentHealth = health.currentHealth;
            int maxHealth = health.GetMaxHealth();
            int healAmount = Mathf.RoundToInt(maxHealth * cardData.valuePerUpgrade);
            int nextHealth = Mathf.Min(currentHealth + healAmount, maxHealth);

            return $"{currentHealth}/{maxHealth} -> {nextHealth}/{maxHealth}";
        }

        float cardCurrentValue = ownedCard.GetCurrentValue();
        float cardNextValue = cardCurrentValue + cardData.valuePerUpgrade;

        return FormatUpgradeValue(cardData, cardCurrentValue, cardNextValue, playerController);
    }

    private CardRarityColor GetRarityColor(CardRarity rarity)
    {
        for (int i = 0; i < rarityColors.Count; i++)
        {
            if (rarityColors[i].rarity == rarity)
                return rarityColors[i];
        }

        return null;
    }

    private string FormatUpgradeValue(CardData cardData, float cardCurrentValue, float cardNextValue, PlayerController playerController)
    {
        switch (cardData.valueDisplayType)
        {
            case CardValueDisplayType.PercentFromDecimal:
                return $"{cardCurrentValue * 100f:0.##}% -> {cardNextValue * 100f:0.##}%";

            case CardValueDisplayType.Multiplier:
                return $"{1f + cardCurrentValue:0.##}x -> {1f + cardNextValue:0.##}x";

            case CardValueDisplayType.FinalFlatStat:
                return FormatFinalFlatStat(cardData, cardCurrentValue, cardNextValue, playerController);

            case CardValueDisplayType.FinalPercentStat:
                return FormatFinalPercentStat(cardData, cardCurrentValue, cardNextValue, playerController);

            case CardValueDisplayType.FinalMultiplierStat:
                return FormatFinalMultiplierStat(cardData, cardCurrentValue, cardNextValue, playerController);

            case CardValueDisplayType.FlatNumber:
            default:
                return $"{cardCurrentValue:0.##} -> {cardNextValue:0.##}";
        }
    }

    private string FormatFinalFlatStat(CardData cardData, float cardCurrentValue, float cardNextValue, PlayerController playerController)
    {
        float currentStat = GetPlayerStatValue(cardData.statType, playerController);

        float currentDisplay = currentStat;
        float nextDisplay = currentStat - cardCurrentValue + cardNextValue;

        return $"{currentDisplay:0.##} -> {nextDisplay:0.##}";
    }

    private string FormatFinalPercentStat(CardData cardData, float cardCurrentValue, float cardNextValue, PlayerController playerController)
    {
        float currentStat = GetPlayerStatValue(cardData.statType, playerController);

        float currentDisplay = currentStat * 100f;
        float nextDisplay = (currentStat - cardCurrentValue + cardNextValue) * 100f;

        return $"{currentDisplay:0.##}% -> {nextDisplay:0.##}%";
    }

    private string FormatFinalMultiplierStat(CardData cardData, float cardCurrentValue, float cardNextValue, PlayerController playerController)
    {
        float currentStat = GetPlayerStatValue(cardData.statType, playerController);

        float currentDisplay = currentStat;
        float nextDisplay = currentStat - cardCurrentValue + cardNextValue;

        return $"{currentDisplay:0.##}x -> {nextDisplay:0.##}x";
    }

    private float GetPlayerStatValue(CardStatType statType, PlayerController playerController)
    {
        if (playerController == null)
            return 0f;

        switch (statType)
        {
            case CardStatType.MaxHealth:
                return playerController.GetMaxHealth();

            case CardStatType.MoveSpeed:
                return playerController.GetMoveSpeed();

            case CardStatType.JumpPower:
                return playerController.GetJumpPower();

            case CardStatType.AttackDamage:
                return playerController.GetAttackDamage();

            case CardStatType.AttackSpeed:
                return playerController.GetAttackSpeed();

            case CardStatType.AttackRange:
                return playerController.GetAttackRange();

            case CardStatType.JumpCount:
                return playerController.GetMaxJumps();

            case CardStatType.Luck:
                return playerController.GetLuck();

            case CardStatType.Knockback:
                return playerController.GetKnockbackStrength();

            case CardStatType.SilverGain:
                return playerController.GetSilverGainMultiplier();

            case CardStatType.CritChance:
                return playerController.GetCritChance();

            case CardStatType.CritDamage:
                return playerController.GetCritDamageMultiplier();

            case CardStatType.WaveHeal:
                return playerController.GetWaveHealPercent();

            case CardStatType.Lifesteal:
                return playerController.GetLifestealPercent();

            case CardStatType.RewardReroll:
                return playerController.GetRewardRerolls();

            default:
                return 0f;
        }
    }
}