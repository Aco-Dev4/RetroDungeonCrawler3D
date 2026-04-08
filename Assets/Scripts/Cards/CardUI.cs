using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CardRarityColor
{
    public CardRarity rarity;
    public Color bannerColor = Color.white;
    public Color backgroundColor = Color.white;
}

public class CardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private Image rarityBanner;
    [SerializeField] private Image background;
    [SerializeField] private Image cardImage;
    [SerializeField] private Button chooseButton;

    [Header("Rarity Colors")]
    [SerializeField] private List<CardRarityColor> rarityColors = new();

    private CardData _cardData;
    private Action<CardData> _onChosen;

    public void Setup(CardData cardData, Action<CardData> onChosen)
    {
        _cardData = cardData;
        _onChosen = onChosen;

        nameText.text = cardData.cardName;
        descriptionText.text = cardData.description;
        rarityText.text = cardData.rarity.ToString();

        CardRarityColor rarityColor = GetRarityColor(cardData.rarity);
        if (rarityColor != null)
        {
            if (rarityBanner != null) rarityBanner.color = rarityColor.bannerColor;
            if (background != null) background.color = rarityColor.backgroundColor;
        }

        if (cardImage != null)
        {
            cardImage.sprite = cardData.icon;
            cardImage.enabled = cardData.icon != null;
        }

        chooseButton.onClick.RemoveAllListeners();
        chooseButton.onClick.AddListener(() => _onChosen?.Invoke(_cardData));
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
}