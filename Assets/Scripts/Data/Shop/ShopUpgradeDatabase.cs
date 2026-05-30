using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopUpgradeDatabase", menuName = "Shop/Upgrade Database")]
public class ShopUpgradeDatabase : ScriptableObject
{
    public List<ShopUpgradeData> upgrades = new();

    public ShopUpgradeData GetUpgrade(string upgradeId)
    {
        for (int i = 0; i < upgrades.Count; i++)
        {
            if (upgrades[i].upgradeId == upgradeId)
                return upgrades[i];
        }

        return null;
    }
}