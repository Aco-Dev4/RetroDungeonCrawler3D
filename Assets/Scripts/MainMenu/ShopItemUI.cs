using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private GameObject goldIcon;
    [SerializeField] private Button buyButton;
    [SerializeField] private Image buyButtonImage;

    [Header("Button Colors")]
    [SerializeField] private Color canAffordColor = Color.green;
    [SerializeField] private Color cannotAffordColor = Color.red;
    [SerializeField] private Color completedColor = Color.cyan;

    private Action _onPressed;

    public void Setup(
    string itemName,
    string description,
    Sprite icon,
    string valueDisplay,
    int cost,
    bool isCompleted,
    bool isOwned,
    string completedText,
    bool canAfford,
    Action onPressed)
    {
        _onPressed = onPressed;

        if (nameText != null)
            nameText.text = itemName;

        if (descriptionText != null)
            descriptionText.text = description;

        if (valueText != null)
            valueText.text = valueDisplay;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (costText != null)
            costText.text = (isCompleted || isOwned)
            ? completedText
            : cost.ToString();

        if (goldIcon != null)
            goldIcon.SetActive(!(isCompleted || isOwned));

        if (buyButtonImage != null)
        {
            if (isCompleted)
                buyButtonImage.color = completedColor;
            else if (isOwned)
                buyButtonImage.color = canAffordColor;
            else
                buyButtonImage.color = canAfford ? canAffordColor : cannotAffordColor;
        }

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.interactable = true;
            buyButton.onClick.AddListener(() => _onPressed?.Invoke());
        }
    }
}