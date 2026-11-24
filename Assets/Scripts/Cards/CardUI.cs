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
    #region References
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    #endregion

    #region UI
    [Header("UI")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private Image rarityBanner;
    [SerializeField] private Image background;
    [SerializeField] private Image cardImage;
    [SerializeField] private CardTypeCatalog cardTypeCatalog;
    private CardTypeBadges _typeBadges;
    [SerializeField] private Button chooseButton;
    [SerializeField] private TMP_Text buttonText;
    [SerializeField] private Image chooseButtonImage;
    #endregion

    #region Reward Colors
    [Header("Reward Colors")]
    [SerializeField] private Color chooseButtonColor = Color.blue;
    [SerializeField] private Color upgradeButtonColor = Color.green;
    [SerializeField] private Color chooseDescriptionColor = Color.white;
    #endregion

    #region Rarity Colors
    [Header("Rarity Colors")]
    [SerializeField] private List<CardRarityColor> rarityColors = new();
    #endregion

    #region Setup
    public void Setup(CardData cardData, Action<CardData> onChosen)
    {
        if (cardData == null) return;

        ApplyBaseCardVisuals(cardData);
        ApplyRewardVisuals(false);

        if (buttonText != null)
            buttonText.text = "CHOOSE";

        if (chooseButton != null)
        {
            chooseButton.onClick.RemoveAllListeners();
            chooseButton.onClick.AddListener(() => onChosen?.Invoke(cardData));
        }
    }

    public void Setup(RewardOption rewardOption, Action<RewardOption> onChosen, PlayerController player)
    {
        if (rewardOption == null || rewardOption.cardData == null)
            return;

        CardData cardData = rewardOption.cardData;
        playerController = player;

        ApplyBaseCardVisuals(cardData);
        ApplyRewardVisuals(rewardOption.isUpgrade);

        if (descriptionText != null)
            descriptionText.text = rewardOption.isUpgrade
                ? GetUpgradePreviewText(cardData)
                : cardData.description;

        if (buttonText != null)
            buttonText.text = rewardOption.isUpgrade ? "UPGRADE" : "CHOOSE";

        if (chooseButton != null)
        {
            chooseButton.onClick.RemoveAllListeners();
            chooseButton.onClick.AddListener(() => onChosen?.Invoke(rewardOption));
        }
    }
    #endregion

    #region Visuals
    private void ApplyBaseCardVisuals(CardData cardData)
    {
        if (nameText != null)
            nameText.text = cardData.cardName;

        if (descriptionText != null)
            descriptionText.text = cardData.description;

        if (rarityText != null)
            rarityText.text = cardData.rarity.ToString();

        ApplyRarityColors(cardData.rarity);
        ApplyIcon(cardData.icon);
        CardTypeBadges.Apply(ref _typeBadges, (RectTransform)transform, cardTypeCatalog, cardData.cardTypes, nameText);
    }

    private void ApplyRewardVisuals(bool isUpgrade)
    {
        if (chooseButtonImage != null)
            chooseButtonImage.color = isUpgrade ? upgradeButtonColor : chooseButtonColor;

        if (descriptionText != null)
            descriptionText.color = isUpgrade ? Color.white : chooseDescriptionColor;
    }

    private void ApplyRarityColors(CardRarity rarity)
    {
        CardRarityColor rarityColor = GetRarityColor(rarity);
        if (rarityColor == null) return;

        if (rarityBanner != null)
            rarityBanner.color = rarityColor.bannerColor;

        if (background != null)
            background.color = rarityColor.backgroundColor;
    }

    private void ApplyIcon(Sprite icon)
    {
        if (cardImage == null) return;

        cardImage.sprite = icon;
        cardImage.enabled = icon != null;
    }
    #endregion

    #region Upgrade Preview
    private string GetUpgradePreviewText(CardData cardData)
    {
        if (cardData == null || playerController == null)
            return "";

        RunCardInventory inventory = playerController.GetComponent<RunCardInventory>();
        if (inventory == null)
            return "";

        OwnedCard ownedCard = inventory.GetOwnedCard(cardData);
        if (ownedCard == null)
            return "";

        string preview = UpgradeComparisonText.HighlightNextValue(
            CardUpgradePreviewFormatter.GetPreviewText(ownedCard, playerController));
        return preview.Contains('\n') ? $"<size=87%>{preview}</size>" : preview;
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
