using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShopUpgradeTier
{
    public int cost;
    public string valueText;
    public int intValue;
    public float floatValue;
}

[Serializable]
public class ShopUpgradeData
{
    public string upgradeId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    public List<ShopUpgradeTier> tiers = new();
}