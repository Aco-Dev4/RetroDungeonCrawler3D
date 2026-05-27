using System;
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

    private OwnedCard _ownedCard;
    private Action<OwnedCard> _onUpgradePressed;

    public void Setup(OwnedCard ownedCard, int upgradeCost, Health health, Action<OwnedCard> onUpgradePressed)
    {
        _ownedCard = ownedCard;
        _onUpgradePressed = onUpgradePressed;

        if (ownedCard == null || ownedCard.cardData == null)
            return;

        CardData cardData = ownedCard.cardData;

        cardNameText.text = cardData.cardName;
        levelText.text = $"LEVEL: {ownedCard.level}";
        costText.text = upgradeCost.ToString();

        if (cardIcon != null)
        {
            cardIcon.sprite = cardData.icon;
            cardIcon.enabled = cardData.icon != null;
        }

        valueText.text = GetUpgradePreviewText(ownedCard, health);

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() => _onUpgradePressed?.Invoke(_ownedCard));
    }

    private string GetUpgradePreviewText(OwnedCard ownedCard, Health health)
    {
        if (ownedCard == null || ownedCard.cardData == null)
            return "";

        CardData cardData = ownedCard.cardData;

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

        float currentValue = ownedCard.GetCurrentValue();
        float nextValue = currentValue + cardData.valuePerUpgrade;

        float currentDisplay = cardData.displayBaseValue + currentValue;
        float nextDisplay = cardData.displayBaseValue + nextValue;

        return $"{currentDisplay:0.##} -> {nextDisplay:0.##}";
    }
}