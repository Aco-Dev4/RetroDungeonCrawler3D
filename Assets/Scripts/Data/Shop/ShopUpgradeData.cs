using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShopUpgradeTier
{
    public int cost;
    [Tooltip("Used when the upgrade displays text instead of a number.")]
    public string valueText;
    public int intValue;
    public float floatValue;
}

public enum ShopUpgradeValueDisplay
{
    Text,
    IntegerBonus,
    PercentageBonus
}

[Serializable]
public class ShopUpgradeData
{
    public string upgradeId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public ShopUpgradeValueDisplay valueDisplay;

    public List<ShopUpgradeTier> tiers = new();

    public string GetTierDisplayText(ShopUpgradeTier tier)
    {
        return valueDisplay switch
        {
            ShopUpgradeValueDisplay.IntegerBonus => $"+{tier.intValue}",
            ShopUpgradeValueDisplay.PercentageBonus => $"+{tier.floatValue * 100f:0.##}%",
            _ => tier.valueText
        };
    }
}