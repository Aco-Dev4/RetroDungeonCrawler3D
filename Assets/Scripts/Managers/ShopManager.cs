using System.Collections.Generic;
using UnityEngine;

public enum ShopTab
{
    Upgrades,
    Colors
}

public class ShopManager : MonoBehaviour
{
    [Header("References")]
    private ShopTab _currentTab = ShopTab.Upgrades;
    [SerializeField] private Transform contentParent;
    [SerializeField] private ShopItemUI shopItemPrefab;
    [SerializeField] private PlayerColorApplier playerColorApplier;

    [Header("Upgrades")]
    [SerializeField] private ShopUpgradeDatabase upgradeDatabase;

    [Header("Colors")]
    [SerializeField] private ShopColorDatabase colorDatabase;

    [Header("Button State Text")]
    [SerializeField] private string maxedUpgradeText = "MAXED";
    [SerializeField] private string equippedColorText = "EQUIPPED";

    private readonly List<ShopItemUI> _spawnedItems = new();

    private void Start()
    {
        ApplySavedColor();
    }

    private void ApplySavedColor()
    {
        if (GameDataManager.Instance == null) return;
        if (playerColorApplier == null) return;
        if (colorDatabase == null) return;

        ShopColorData selectedColor = colorDatabase.GetColor(GameDataManager.Instance.GetSelectedColor());

        if (selectedColor != null)
            playerColorApplier.ApplyShopColor(selectedColor);
    }

    private void OnEnable()
    {
        ShowUpgradesTab();
    }

    public void ShowUpgradesTab()
    {
        _currentTab = ShopTab.Upgrades;
        ShowUpgrades();
    }

    public void ShowColorsTab()
    {
        _currentTab = ShopTab.Colors;
        ClearItems();

        for (int i = 0; i < colorDatabase.colors.Count; i++)
            CreateColorItem(colorDatabase.colors[i]);
    }

    public void ShowUpgrades()
    {
        ClearItems();

        if (upgradeDatabase == null) return;

        for (int i = 0; i < upgradeDatabase.upgrades.Count; i++)
            CreateUpgradeItem(upgradeDatabase.upgrades[i]);
    }

    private void CreateUpgradeItem(ShopUpgradeData upgrade)
    {
        if (upgrade == null || shopItemPrefab == null || contentParent == null) return;

        int currentTier = GetUpgradeTier(upgrade.upgradeId);
        bool isMaxed = currentTier >= upgrade.tiers.Count - 1;

        ShopUpgradeTier currentTierData = upgrade.tiers[Mathf.Clamp(currentTier, 0, upgrade.tiers.Count - 1)];
        ShopUpgradeTier nextTierData = isMaxed ? null : upgrade.tiers[currentTier + 1];

        int cost = isMaxed ? 0 : nextTierData.cost;
        string valueText = currentTierData.valueText;

        ShopItemUI item = Instantiate(shopItemPrefab, contentParent);

        int currentGold = GameDataManager.Instance != null ? GameDataManager.Instance.GetGold() : 0;
        bool canAfford = currentGold >= cost;

        item.Setup(
            upgrade.displayName,
            upgrade.description,
            upgrade.icon,
            valueText,
            cost,
            isMaxed,
            false,
            maxedUpgradeText,
            canAfford,
            () => BuyUpgrade(upgrade)
        );

        _spawnedItems.Add(item);
    }

    private void BuyUpgrade(ShopUpgradeData upgrade)
    {
        if (GameDataManager.Instance == null) return;

        int currentTier = GetUpgradeTier(upgrade.upgradeId);
        if (currentTier >= upgrade.tiers.Count - 1)
        {
            AudioManager.Instance?.PlaySFX("UICancel");
            return;
        }

        int cost = upgrade.tiers[currentTier + 1].cost;

        if (!GameDataManager.Instance.SpendGold(cost))
        {
            AudioManager.Instance?.PlaySFX("UICancel");
            return;
        }

        GameDataManager.Instance.SetUpgradeTier(upgrade.upgradeId, currentTier + 1);
        AudioManager.Instance?.PlaySFX("UIBuy");
        GoldUI.Instance?.SetGold(GameDataManager.Instance.GetGold());

        //Debug.Log($"{upgrade.displayName} bought. Tier {currentTier + 1}");

        ShowUpgrades();
    }

    private int GetUpgradeTier(string upgradeId)
    {
        if (GameDataManager.Instance == null) return 0;
        return GameDataManager.Instance.GetUpgradeTier(upgradeId);
    }

    private void ClearItems()
    {
        for (int i = _spawnedItems.Count - 1; i >= 0; i--)
        {
            if (_spawnedItems[i] != null)
                Destroy(_spawnedItems[i].gameObject);
        }

        _spawnedItems.Clear();
    }

    private void CreateColorItem(ShopColorData colorData)
    {
        if (colorData == null || shopItemPrefab == null || contentParent == null) return;
        if (GameDataManager.Instance == null) return;

        bool isOwned = GameDataManager.Instance.HasColor(colorData.colorId);
        bool isSelected = GameDataManager.Instance.GetSelectedColor() == colorData.colorId;

        int currentGold = GameDataManager.Instance.GetGold();
        bool canAfford = isOwned || currentGold >= colorData.cost;

        string valueText = isSelected ? "SELECTED" : isOwned ? "OWNED" : "LOCKED";

        string buttonText = isSelected ? "EQUIPPED" : isOwned ? "EQUIP" : equippedColorText;
        int shownCost = isOwned ? 0 : colorData.cost;

        bool hideGoldIcon = isSelected || isOwned;

        ShopItemUI item = Instantiate(shopItemPrefab, contentParent);

        item.Setup(
            colorData.displayName,
            "Change your knight color.",
            colorData.icon,
            valueText,
            shownCost,
            isSelected,
            isOwned,
            buttonText,
            canAfford,
            () => BuyOrSelectColor(colorData)
        );

        _spawnedItems.Add(item);
    }

    private void BuyOrSelectColor(ShopColorData colorData)
    {
        if (GameDataManager.Instance == null) return;

        bool isOwned = GameDataManager.Instance.HasColor(colorData.colorId);

        if (!isOwned)
        {
            if (!GameDataManager.Instance.SpendGold(colorData.cost))
            {
                AudioManager.Instance?.PlaySFX("UICancel");
                return;
            }

            GameDataManager.Instance.BuyColor(colorData.colorId);
            GoldUI.Instance?.SetGold(GameDataManager.Instance.GetGold());
        }

        GameDataManager.Instance.SetSelectedColor(colorData.colorId);
        AudioManager.Instance?.PlaySFX("UIBuy");
        playerColorApplier?.ApplyShopColor(colorData);
        //Debug.Log($"Selected color: {colorData.displayName}");

        ShowColorsTab();
    }
}