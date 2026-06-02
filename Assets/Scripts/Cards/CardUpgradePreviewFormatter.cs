using UnityEngine;

public static class CardUpgradePreviewFormatter
{
    public static string GetPreviewText(OwnedCard ownedCard, PlayerController playerController)
    {
        if (ownedCard == null || ownedCard.cardData == null)
            return "";

        CardData cardData = ownedCard.cardData;
        Health health = playerController != null ? playerController.GetComponent<Health>() : null;

        if (cardData.statType == CardStatType.Heal)
            return GetHealPreview(cardData, health);

        float currentValue = ownedCard.GetCurrentValue();
        float nextValue = currentValue + cardData.valuePerUpgrade;

        return FormatUpgradeValue(cardData, currentValue, nextValue, playerController);
    }

    private static string GetHealPreview(CardData cardData, Health health)
    {
        if (health == null)
            return $"Heal {cardData.valuePerUpgrade * 100f:0}%";

        int currentHealth = health.currentHealth;
        int maxHealth = health.GetMaxHealth();
        int healAmount = Mathf.RoundToInt(maxHealth * cardData.valuePerUpgrade);
        int nextHealth = Mathf.Min(currentHealth + healAmount, maxHealth);

        return $"{currentHealth}/{maxHealth} -> {nextHealth}/{maxHealth}";
    }

    private static string FormatUpgradeValue(CardData cardData, float currentValue, float nextValue, PlayerController playerController)
    {
        switch (cardData.valueDisplayType)
        {
            case CardValueDisplayType.PercentFromDecimal:
                return $"{currentValue * 100f:0.##}% -> {nextValue * 100f:0.##}%";

            case CardValueDisplayType.Multiplier:
                return $"{1f + currentValue:0.##}x -> {1f + nextValue:0.##}x";

            case CardValueDisplayType.FinalFlatStat:
                return FormatFinalFlatStat(cardData, currentValue, nextValue, playerController);

            case CardValueDisplayType.FinalPercentStat:
                return FormatFinalPercentStat(cardData, currentValue, nextValue, playerController);

            case CardValueDisplayType.FinalMultiplierStat:
                return FormatFinalMultiplierStat(cardData, currentValue, nextValue, playerController);

            case CardValueDisplayType.FlatNumber:
            default:
                return $"{currentValue:0.##} -> {nextValue:0.##}";
        }
    }

    private static string FormatFinalFlatStat(CardData cardData, float currentValue, float nextValue, PlayerController playerController)
    {
        float currentStat = GetPlayerStatValue(cardData.statType, playerController);
        float nextStat = currentStat - currentValue + nextValue;

        return $"{currentStat:0.##} -> {nextStat:0.##}";
    }

    private static string FormatFinalPercentStat(CardData cardData, float currentValue, float nextValue, PlayerController playerController)
    {
        float currentStat = GetPlayerStatValue(cardData.statType, playerController);
        float nextStat = currentStat - currentValue + nextValue;

        return $"{currentStat * 100f:0.##}% -> {nextStat * 100f:0.##}%";
    }

    private static string FormatFinalMultiplierStat(CardData cardData, float currentValue, float nextValue, PlayerController playerController)
    {
        float currentStat = GetPlayerStatValue(cardData.statType, playerController);
        float nextStat = currentStat - currentValue + nextValue;

        return $"{currentStat:0.##}x -> {nextStat:0.##}x";
    }

    private static float GetPlayerStatValue(CardStatType statType, PlayerController playerController)
    {
        if (playerController == null)
            return 0f;

        switch (statType)
        {
            case CardStatType.MaxHealth:
                return playerController.GetMaxHealth();
            case CardStatType.MoveSpeed:
                return playerController.GetMoveSpeed();
            case CardStatType.JumpPower:
                return playerController.GetJumpPower();
            case CardStatType.AttackDamage:
                return playerController.GetAttackDamage();
            case CardStatType.AttackSpeed:
                return playerController.GetAttackSpeed();
            case CardStatType.AttackRange:
                return playerController.GetAttackRange();
            case CardStatType.JumpCount:
                return playerController.GetMaxJumps();
            case CardStatType.Luck:
                return playerController.GetLuck();
            case CardStatType.Knockback:
                return playerController.GetKnockbackStrength();
            case CardStatType.SilverGain:
                return playerController.GetSilverGainMultiplier();
            case CardStatType.CritChance:
                return playerController.GetCritChance();
            case CardStatType.CritDamage:
                return playerController.GetCritDamageMultiplier();
            case CardStatType.WaveHeal:
                return playerController.GetWaveHealPercent();
            case CardStatType.Lifesteal:
                return playerController.GetLifestealPercent();
            case CardStatType.RewardReroll:
                return playerController.GetRewardRerolls();
            default:
                return 0f;
        }
    }
}