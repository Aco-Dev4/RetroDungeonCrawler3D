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
            if (upgrades[i] != null && upgrades[i].upgradeId == upgradeId)
                return upgrades[i];
        }

        return null;
    }

    public ShopUpgradeTier GetTier(string upgradeId, int tierIndex)
    {
        ShopUpgradeData upgrade = GetUpgrade(upgradeId);
        if (upgrade == null || upgrade.tiers.Count == 0) return null;

        return upgrade.tiers[Mathf.Clamp(tierIndex, 0, upgrade.tiers.Count - 1)];
    }
}