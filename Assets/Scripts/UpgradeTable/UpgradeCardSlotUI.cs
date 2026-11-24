using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeCardSlotUI : MonoBehaviour
{
    #region UI
    [Header("UI")]
    [SerializeField] private TMP_Text cardNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Image cardIcon;
    [SerializeField] private CardTypeCatalog cardTypeCatalog;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image background;
    [SerializeField] private Image upgradeButtonImage;
    #endregion

    #region Colors
    [Header("Rarity Colors")]
    [SerializeField] private List<CardRarityColor> rarityColors = new();

    [Header("Button Colors")]
    [SerializeField] private Color canAffordColor = Color.green;
    [SerializeField] private Color cannotAffordColor = Color.red;
    #endregion

    #region Runtime
    private OwnedCard _ownedCard;
    private CardTypeBadges _typeBadges;
    private Vector2 _valueTextSize;
    private Vector2 _valueTextPosition;
    private float _valueFontSize;
    private Action<OwnedCard> _onUpgradePressed;
    #endregion

    private void Awake()
    {
        if (valueText == null) return;

        _valueTextSize = valueText.rectTransform.sizeDelta;
        _valueTextPosition = valueText.rectTransform.anchoredPosition;
        _valueFontSize = valueText.fontSize;
    }

    #region Setup
    public void Setup(OwnedCard ownedCard, int upgradeCost, int currentSilver, PlayerController playerController, Action<OwnedCard> onUpgradePressed)
    {
        if (ownedCard == null || ownedCard.cardData == null)
            return;

        _ownedCard = ownedCard;
        _onUpgradePressed = onUpgradePressed;

        CardData cardData = ownedCard.cardData;
        bool canAfford = currentSilver >= upgradeCost;

        ApplyTexts(ownedCard, upgradeCost, playerController);
        ApplyIcon(cardData.icon);
        CardTypeBadges.Apply(ref _typeBadges, (RectTransform)transform, cardTypeCatalog, cardData.cardTypes, cardNameText);
        ApplyRarityColor(cardData.rarity);
        ApplyButtonColor(canAfford);
        SetupButton();
    }
    #endregion

    #region Visuals
    private void ApplyTexts(OwnedCard ownedCard, int upgradeCost, PlayerController playerController)
    {
        if (cardNameText != null)
            cardNameText.text = ownedCard.cardData.cardName;

        if (levelText != null)
            levelText.text = $"LEVEL: {ownedCard.level}";

        if (costText != null)
            costText.text = upgradeCost.ToString();

        if (valueText != null)
        {
            string preview = CardUpgradePreviewFormatter.GetPreviewText(ownedCard, playerController);
            bool multiline = preview.Contains('\n');
            valueText.fontSize = multiline ? 10f : _valueFontSize;
            valueText.rectTransform.sizeDelta = _valueTextSize + Vector2.up * (multiline ? 14f : 0f);
            valueText.rectTransform.anchoredPosition = _valueTextPosition + Vector2.up * (multiline ? 7f : 0f);
            valueText.text = UpgradeComparisonText.HighlightNextValue(preview);
        }
    }

    private void ApplyIcon(Sprite icon)
    {
        if (cardIcon == null) return;

        cardIcon.sprite = icon;
        cardIcon.enabled = icon != null;
    }

    private void ApplyRarityColor(CardRarity rarity)
    {
        CardRarityColor rarityColor = GetRarityColor(rarity);

        if (rarityColor != null && background != null)
            background.color = rarityColor.backgroundColor;
    }

    private void ApplyButtonColor(bool canAfford)
    {
        if (upgradeButtonImage != null)
            upgradeButtonImage.color = canAfford ? canAffordColor : cannotAffordColor;
    }
    #endregion

    #region Button
    private void SetupButton()
    {
        if (upgradeButton == null) return;

        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() => _onUpgradePressed?.Invoke(_ownedCard));
    }
    #endregion

    #region Helpers
    private CardRarityColor GetRarityColor(CardRarity rarity)
    {
        for (int i = 0; i < rarityColors.Count; i++)
        {
            if (rarityColors[i].rarity == rarity)
                return rarityColors[i];
        }

        return null;
    }
    #endregion
}