using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    [Header("Info")]
    public string cardName;
    [TextArea] public string description;
    public Sprite icon;
    public CardRarity rarity;
    public CardStatType statType;

    [Header("Values")]
    public bool usePercent;
    public float baseValue;
    public float valuePerUpgrade = 0.05f;

    [Header("Upgrade Cost")]
    public int baseUpgradeCost = 10;
    public int costIncreasePerLevel = 10;

    [Header("Upgrade Display")]
    public CardValueDisplayType valueDisplayType = CardValueDisplayType.FlatNumber;

    [Header("Rules")]
    public bool canAppearInChest = true;
    public bool requiresWeapon;
    public bool requiresCriticalHits;
}